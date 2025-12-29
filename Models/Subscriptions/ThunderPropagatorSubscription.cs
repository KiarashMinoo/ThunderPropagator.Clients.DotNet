using Ardalis.GuardClauses;
using ThunderPropagator.BuildingBlocks.Application.Collections;
using ThunderPropagator.Clients.DotNet.Infrastructure.Channels;
using ThunderPropagator.Clients.DotNet.Infrastructure.Loggers;
using ThunderPropagator.Clients.DotNet.Models.Enums;
using ThunderPropagator.Clients.DotNet.Models.ReceivedMessage;
using ThunderPropagator.Clients.DotNet.Models.Requests;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ThunderPropagator.Clients.DotNet.Models.Subscriptions
{
    public delegate void ThunderPropagatorSubscriptionStatusChangedHandler(object sender, ThunderPropagatorSubscriptionStatus status, EventArgs args);

    public delegate Task ThunderPropagatorFieldUpdatedEventHandler(object sender, ThunderPropagatorSubscriptionItemUpdate item, CancellationToken cancellationToken = default);

    public delegate Task ThunderPropagatorTableUpdatedEventHandler
    (object sender, string table, string key, ICollection<ThunderPropagatorSubscriptionItemUpdate> items, ThunderPropagatorRecordStatus recordStatus,
        CancellationToken cancellationToken = default);

    public
#if !DEBUG
        sealed
#endif
        class ThunderPropagatorSubscription : INotifyPropertyChanged,
        IThunderPropagatorSubscriptionOperational
    {
        private const string DefaultTable = "DEFAULT";

        private readonly IThunderPropagatorChannel _thunderPropagatorChannel;
        private readonly ILogger _logger;
        private readonly Dictionary<string, ClientSubscriptionTable> _tables;
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        private ThunderPropagatorSubscriptionStatus _subscriptionStatus = ThunderPropagatorSubscriptionStatus.Initiated;

        public IReadOnlyDictionary<string, string> SubscribingKeys { get; }
        public IReadOnlyCollection<string> SubscribingFields { get; }
        public ThunderPropagatorSubscriptionMode SubscriptionMode { get; }

        public ThunderPropagatorSubscriptionStatus SubscriptionStatus
        {
            get => _subscriptionStatus;
            internal set
            {
                if (SetField(ref _subscriptionStatus, value))
                    StatusChanged?.Invoke(this, _subscriptionStatus, EventArgs.Empty);
            }
        }

        public event ThunderPropagatorSubscriptionStatusChangedHandler? StatusChanged;
        public event ThunderPropagatorFieldUpdatedEventHandler? FieldUpdated;
        public event ThunderPropagatorTableUpdatedEventHandler? TableUpdated;
        public event PropertyChangedEventHandler? PropertyChanged;

        internal ThunderPropagatorSubscription(IThunderPropagatorChannel thunderPropagatorChannel,
            IReadOnlyDictionary<string, string> subscribingKeys,
            IReadOnlyCollection<string> subscribingFields,
            ThunderPropagatorSubscriptionMode subscriptionMode,
            ILoggerProvider loggerProvider)
        {
            _thunderPropagatorChannel = thunderPropagatorChannel ?? throw new ArgumentNullException(nameof(thunderPropagatorChannel));
            ArgumentNullException.ThrowIfNull(_thunderPropagatorChannel.ChannelMetadata, nameof(_thunderPropagatorChannel.ChannelMetadata));

            SubscribingKeys = subscribingKeys;
            SubscribingFields = subscribingFields;
            SubscriptionMode = subscriptionMode;
            _logger = loggerProvider.CreateLogger(GetType().GetTypeInfo().Name);

            _tables = _thunderPropagatorChannel.ChannelMetadata.ChannelProgramsDescriptors.Values.GroupBy(cpd => cpd.Table ?? DefaultTable).Select(table =>
            {
                var subscribingKey = table.Single(cpd => cpd.IsSubscribingKey);
                Func<IReadOnlyDictionary<int, string>, string> tableKey =
                    SubscribingKeys.TryGetValue(subscribingKey.Name, out var subscribingKeyValue)
                        ? _ => Guard.Against.NullOrWhiteSpace(subscribingKeyValue)
                        : values => Guard.Against.NullOrWhiteSpace(values[subscribingKey.Index]);

                return new ClientSubscriptionTable
                {
                    TableName = table.Key,
                    TableKey = tableKey
                };
            }).ToDictionary(table => table.TableName, table => table);

            _logger.Log(LogLevel.Information, null, "Subscription object has created");
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public async Task SubscribeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                SubscriptionStatus = ThunderPropagatorSubscriptionStatus.Subscribing;

                await _thunderPropagatorChannel.RequestSubscriptionAsync(BuildRequest(), cancellationToken: cancellationToken);
            }
            catch (Exception exception)
            {
                SubscriptionStatus = ThunderPropagatorSubscriptionStatus.HasError;
                _logger.Log(LogLevel.Error, exception, "An error occured while subscribing subscription");
            }

            return;

            ThunderPropagatorSubscriptionRequest BuildRequest()
                => new(this,
                    RequestIdHelper.Generate(),
                    Guard.Against.Null(_thunderPropagatorChannel.ChannelMetadata, nameof(_thunderPropagatorChannel.ChannelMetadata)).ChannelName,
                    [SubscribingKeys],
                    SubscribingFields,
                    SubscriptionMode);
        }

        public async Task UnsubscribeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                SubscriptionStatus = ThunderPropagatorSubscriptionStatus.Unsubscribing;

                await _thunderPropagatorChannel.RequestUnsubscribeAsync(BuildRequest(), cancellationToken: cancellationToken);

                SubscriptionStatus = ThunderPropagatorSubscriptionStatus.Unsubscribed;
            }
            catch (Exception exception)
            {
                SubscriptionStatus = ThunderPropagatorSubscriptionStatus.HasError;
                _logger.Log(LogLevel.Error, exception, "An error occured while unsubscribing subscription");
            }

            return;

            ThunderPropagatorUnsubscribeRequest BuildRequest()
                => new(this,
                    RequestIdHelper.Generate(),
                    Guard.Against.Null(_thunderPropagatorChannel.ChannelMetadata, nameof(_thunderPropagatorChannel.ChannelMetadata)).ChannelName,
                    [SubscribingKeys]);
        }


        internal async Task HandleReceivedMessageAsync(ChannelReceivedMessage receivedMessage, CancellationToken cancellationToken = default)
        {
            if (FieldUpdated is null && TableUpdated is null)
                return;

            ArgumentNullException.ThrowIfNull(_thunderPropagatorChannel.ChannelMetadata, nameof(_thunderPropagatorChannel.ChannelMetadata));

            await _semaphoreSlim.WaitAsync(cancellationToken);

            try
            {
                BindingDictionary<ClientSubscriptionTable, HashSet<string>> tablesChanged = [];

                foreach (var messageValue in receivedMessage.Values)
                {
                    var cpd = Guard.Against.Null(_thunderPropagatorChannel.ChannelMetadata[messageValue.Key],
                        message: $"Channel program descriptor could not be found for message value index {messageValue.Key}");

                    var table = _tables[cpd.Table ?? DefaultTable];
                    var key = table.TableKey.Invoke(receivedMessage.Values);

                    var flag = false;
                    if (receivedMessage.Header.RecordStatus == "D" && table.Table.Remove(key))
                        flag = true;
                    else
                    {
                        var record = table.Table.GetValueOrAdd(key, () => []);
                        var item = record.GetValueOrAdd(cpd.Index, () =>
                        {
                            flag = true;
                            return new ThunderPropagatorSubscriptionItemUpdate
                            {
                                Keys = receivedMessage.Keys,
                                FieldIndex = cpd.Index,
                                FieldName = cpd.Name,
                                FieldType = cpd.Type
                            };
                        });

                        item.IsSnapshot = receivedMessage.Header.FromSnapshot;
                        item.FieldValue = messageValue.Value;

                        if (item.Changed)
                        {
                            flag = true;

                            if (FieldUpdated is not null && receivedMessage.Header.RecordStatus is "M" or "N")
                                await FieldUpdated.Invoke(this, item, cancellationToken);
                        }
                    }

                    if (flag)
                    {
                        tablesChanged.AddOrUpdate(table, () => [key], set =>
                        {
                            set.Add(key);
                            return set;
                        });
                    }
                }

                if (TableUpdated is not null && tablesChanged.Count > 0)
                {
                    var recordStatus = receivedMessage.Header.RecordStatus switch
                    {
                        "A" => ThunderPropagatorRecordStatus.Added,
                        "M" => ThunderPropagatorRecordStatus.Modified,
                        "D" => ThunderPropagatorRecordStatus.Deleted,
                        "N" => ThunderPropagatorRecordStatus.Neutral,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    foreach (var table in tablesChanged)
                    {
                        foreach (var key in table.Value)
                        {
                            await TableUpdated.Invoke(this,
                                table.Key.TableName,
                                key,
                                recordStatus == ThunderPropagatorRecordStatus.Deleted ? Array.Empty<ThunderPropagatorSubscriptionItemUpdate>() : table.Key.Table[key].Values,
                                recordStatus,
                                cancellationToken);
                        }
                    }
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}