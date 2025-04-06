using RapidStreamer.Clients.DotNet.Infrastructure.Channels;
using RapidStreamer.Clients.DotNet.Infrastructure.Loggers;
using RapidStreamer.Clients.DotNet.Models.Enums;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RapidStreamer.Clients.DotNet.Models.Subscriptions
{
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerSubscriptions : INotifyPropertyChanged,
        IRapidStreamerSubscriptionOperational
    {
        private readonly HashSet<RapidStreamerSubscription> _subscriptions;
        private readonly ILogger _logger;
        private RapidStreamerSubscriptionStatus _subscriptionStatus = RapidStreamerSubscriptionStatus.Initiated;

        public IReadOnlyCollection<RapidStreamerSubscription> Subscriptions => _subscriptions;
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

        internal RapidStreamerSubscriptions(IRapidStreamerChannel rapidStreamerChannel,
            IReadOnlyCollection<IReadOnlyDictionary<string, string>> subscribingKeys,
            IReadOnlyCollection<string> subscribingFields,
            RapidStreamerSubscriptionMode subscriptionMode,
            ILoggerProvider loggerProvider)
        {
            SubscriptionMode = subscriptionMode;
            _logger = loggerProvider.CreateLogger(GetType().GetTypeInfo().Name);

            _subscriptions = [];
            foreach (var subscribingKey in subscribingKeys)
                _subscriptions.Add(new RapidStreamerSubscription(rapidStreamerChannel, subscribingKey, subscribingFields, SubscriptionMode, loggerProvider));

            _logger.Log(LogLevel.Information, null, "Subscriptions object has created");
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

        private async Task SubscriptionOnTableUpdated(object sender,
            string table,
            string key,
            ICollection<RapidStreamerSubscriptionItemUpdate> items,
            RapidStreamerRecordStatus recordStatus,
            CancellationToken cancellationToken)
        {
            if (TableUpdated is not null)
                await TableUpdated.Invoke(this, table, key, items, recordStatus, cancellationToken);
        }

        private async Task SubscriptionOnFieldUpdated(object sender, RapidStreamerSubscriptionItemUpdate item, CancellationToken cancellationToken)
        {
            if (FieldUpdated is not null)
                await FieldUpdated.Invoke(this, item, cancellationToken);
        }

        public async Task SubscribeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                SubscriptionStatus = RapidStreamerSubscriptionStatus.Subscribing;

                await Task.WhenAll(Subscriptions.Select(subscription =>
                {
                    subscription.FieldUpdated += SubscriptionOnFieldUpdated;
                    subscription.TableUpdated += SubscriptionOnTableUpdated;
                    return subscription.SubscribeAsync(cancellationToken);
                }));
            }
            catch (Exception exception)
            {
                SubscriptionStatus = RapidStreamerSubscriptionStatus.HasError;
                _logger.Log(LogLevel.Error, exception, "An error occured while subscribing subscription");
            }
        }

        public async Task UnsubscribeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                SubscriptionStatus = RapidStreamerSubscriptionStatus.Unsubscribing;

                await Task.WhenAll(Subscriptions.Select(subscription => subscription.UnsubscribeAsync(cancellationToken)));

                SubscriptionStatus = RapidStreamerSubscriptionStatus.Unsubscribed;
            }
            catch (Exception exception)
            {
                SubscriptionStatus = RapidStreamerSubscriptionStatus.HasError;
                _logger.Log(LogLevel.Error, exception, "An error occured while unsubscribing subscription");
            }
        }
    }
}