using Ardalis.GuardClauses;
using RapidStreamer.BuildingBlocks.Application.Collections;
using RapidStreamer.Clients.DotNet.Infrastructure.Channels;
using RapidStreamer.Clients.DotNet.Infrastructure.Loggers;
using RapidStreamer.Clients.DotNet.Models.Enums;
using RapidStreamer.Clients.DotNet.Models.ReceivedMessage;
using RapidStreamer.Clients.DotNet.Models.Requests;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RapidStreamer.Clients.DotNet.Models.Subscriptions
{
    public delegate void RapidStreamerSubscriptionStatusChangedHandler(object sender, RapidStreamerSubscriptionStatus status, EventArgs args);

    public delegate Task RapidStreamerFieldUpdatedEventHandler(object sender, RapidStreamerSubscriptionItemUpdate item, CancellationToken cancellationToken = default);

    public delegate Task RapidStreamerTableUpdatedEventHandler
    (object sender, string table, string key, ICollection<RapidStreamerSubscriptionItemUpdate> items, RapidStreamerRecordStatus recordStatus,
        CancellationToken cancellationToken = default);

    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerSubscription : INotifyPropertyChanged,
        IRapidStreamerSubscriptionOperational
    {
        private const string DefaultTable = "DEFAULT";

        private readonly IRapidStreamerChannel _rapidStreamerChannel;
        private readonly ILogger _logger;
        private readonly Dictionary<string, ClientSubscriptionTable> _tables;
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        private RapidStreamerSubscriptionStatus _subscriptionStatus = RapidStreamerSubscriptionStatus.Initiated;

        public IReadOnlyDictionary<string, string> SubscribingKeys { get; }
        public IReadOnlyCollection<string> SubscribingFields { get; }
        public RapidStreamerSubscriptionMode SubscriptionMode { get; }

        public RapidStreamerSubscriptionStatus SubscriptionStatus
        {
            get => _subscriptionStatus;
            internal set
            {
                if (SetField(ref _subscriptionStatus, value))
                    StatusChanged?.Invoke(this, _subscriptionStatus, EventArgs.Empty);
            }
        }

        public event RapidStreamerSubscriptionStatusChangedHandler? StatusChanged;
        public event RapidStreamerFieldUpdatedEventHandler? FieldUpdated;
        public event RapidStreamerTableUpdatedEventHandler? TableUpdated;
        public event PropertyChangedEventHandler? PropertyChanged;

        internal RapidStreamerSubscription(IRapidStreamerChannel rapidStreamerChannel,
            IReadOnlyDictionary<string, string> subscribingKeys,
            IReadOnlyCollection<string> subscribingFields,
            RapidStreamerSubscriptionMode subscriptionMode,
            ILoggerProvider loggerProvider)
        {
            _rapidStreamerChannel = rapidStreamerChannel ?? throw new ArgumentNullException(nameof(rapidStreamerChannel));
            ArgumentNullException.ThrowIfNull(_rapidStreamerChannel.ChannelMetadata, nameof(_rapidStreamerChannel.ChannelMetadata));

            SubscribingKeys = subscribingKeys;
            SubscribingFields = subscribingFields;
            SubscriptionMode = subscriptionMode;
            _logger = loggerProvider.CreateLogger(GetType().GetTypeInfo().Name);

            _tables = _rapidStreamerChannel.ChannelMetadata.ChannelProgramsDescriptors.Values.GroupBy(cpd => cpd.Table ?? DefaultTable).Select(table =>
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
                SubscriptionStatus = RapidStreamerSubscriptionStatus.Subscribing;

                await _rapidStreamerChannel.RequestSubscriptionAsync(BuildRequest(), cancellationToken: cancellationToken);
            }
            catch (Exception exception)
            {
                SubscriptionStatus = RapidStreamerSubscriptionStatus.HasError;
                _logger.Log(LogLevel.Error, exception, "An error occured while subscribing subscription");
            }

            return;

            RapidStreamerSubscriptionRequest BuildRequest()
                => new(this,
                    RequestIdHelper.Generate(),
                    Guard.Against.Null(_rapidStreamerChannel.ChannelMetadata, nameof(_rapidStreamerChannel.ChannelMetadata)).ChannelName,
                    [SubscribingKeys],
                    SubscribingFields,
                    SubscriptionMode);
        }

        public async Task UnsubscribeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                SubscriptionStatus = RapidStreamerSubscriptionStatus.Unsubscribing;

                await _rapidStreamerChannel.RequestUnsubscribeAsync(BuildRequest(), cancellationToken: cancellationToken);

                SubscriptionStatus = RapidStreamerSubscriptionStatus.Unsubscribed;
            }
            catch (Exception exception)
            {
                SubscriptionStatus = RapidStreamerSubscriptionStatus.HasError;
                _logger.Log(LogLevel.Error, exception, "An error occured while unsubscribing subscription");
            }

            return;

            RapidStreamerUnsubscribeRequest BuildRequest()
                => new(this,
                    RequestIdHelper.Generate(),
                    Guard.Against.Null(_rapidStreamerChannel.ChannelMetadata, nameof(_rapidStreamerChannel.ChannelMetadata)).ChannelName,
                    [SubscribingKeys]);
        }


        internal async Task HandleReceivedMessageAsync(ChannelReceivedMessage receivedMessage, CancellationToken cancellationToken = default)
        {
            if (FieldUpdated is null && TableUpdated is null)
                return;

            ArgumentNullException.ThrowIfNull(_rapidStreamerChannel.ChannelMetadata, nameof(_rapidStreamerChannel.ChannelMetadata));

            await _semaphoreSlim.WaitAsync(cancellationToken);

            try
            {
                BindingDictionary<ClientSubscriptionTable, HashSet<string>> tablesChanged = [];

                foreach (var messageValue in receivedMessage.Values)
                {
                    var cpd = Guard.Against.Null(_rapidStreamerChannel.ChannelMetadata[messageValue.Key],
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
                            return new RapidStreamerSubscriptionItemUpdate
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
                        "A" => RapidStreamerRecordStatus.Added,
                        "M" => RapidStreamerRecordStatus.Modified,
                        "D" => RapidStreamerRecordStatus.Deleted,
                        "N" => RapidStreamerRecordStatus.Neutral,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    foreach (var table in tablesChanged)
                    {
                        foreach (var key in table.Value)
                        {
                            await TableUpdated.Invoke(this,
                                table.Key.TableName,
                                key,
                                recordStatus == RapidStreamerRecordStatus.Deleted ? Array.Empty<RapidStreamerSubscriptionItemUpdate>() : table.Key.Table[key].Values,
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