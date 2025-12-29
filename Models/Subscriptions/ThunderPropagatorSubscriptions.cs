using ThunderPropagator.Clients.DotNet.Infrastructure.Channels;
using ThunderPropagator.Clients.DotNet.Infrastructure.Loggers;
using ThunderPropagator.Clients.DotNet.Models.Enums;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ThunderPropagator.Clients.DotNet.Models.Subscriptions
{
    public
#if !DEBUG
        sealed
#endif
        class ThunderPropagatorSubscriptions : INotifyPropertyChanged,
        IThunderPropagatorSubscriptionOperational
    {
        private readonly HashSet<ThunderPropagatorSubscription> _subscriptions;
        private readonly ILogger _logger;
        private ThunderPropagatorSubscriptionStatus _subscriptionStatus = ThunderPropagatorSubscriptionStatus.Initiated;

        public IReadOnlyCollection<ThunderPropagatorSubscription> Subscriptions => _subscriptions;
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

        internal ThunderPropagatorSubscriptions(IThunderPropagatorChannel thunderPropagatorChannel,
            IReadOnlyCollection<IReadOnlyDictionary<string, string>> subscribingKeys,
            IReadOnlyCollection<string> subscribingFields,
            ThunderPropagatorSubscriptionMode subscriptionMode,
            ILoggerProvider loggerProvider)
        {
            SubscriptionMode = subscriptionMode;
            _logger = loggerProvider.CreateLogger(GetType().GetTypeInfo().Name);

            _subscriptions = [];
            foreach (var subscribingKey in subscribingKeys)
                _subscriptions.Add(new ThunderPropagatorSubscription(thunderPropagatorChannel, subscribingKey, subscribingFields, SubscriptionMode, loggerProvider));

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
            ICollection<ThunderPropagatorSubscriptionItemUpdate> items,
            ThunderPropagatorRecordStatus recordStatus,
            CancellationToken cancellationToken)
        {
            if (TableUpdated is not null)
                await TableUpdated.Invoke(this, table, key, items, recordStatus, cancellationToken);
        }

        private async Task SubscriptionOnFieldUpdated(object sender, ThunderPropagatorSubscriptionItemUpdate item, CancellationToken cancellationToken)
        {
            if (FieldUpdated is not null)
                await FieldUpdated.Invoke(this, item, cancellationToken);
        }

        public async Task SubscribeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                SubscriptionStatus = ThunderPropagatorSubscriptionStatus.Subscribing;

                await Task.WhenAll(Subscriptions.Select(subscription =>
                {
                    subscription.FieldUpdated += SubscriptionOnFieldUpdated;
                    subscription.TableUpdated += SubscriptionOnTableUpdated;
                    return subscription.SubscribeAsync(cancellationToken);
                }));
            }
            catch (Exception exception)
            {
                SubscriptionStatus = ThunderPropagatorSubscriptionStatus.HasError;
                _logger.Log(LogLevel.Error, exception, "An error occured while subscribing subscription");
            }
        }

        public async Task UnsubscribeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                SubscriptionStatus = ThunderPropagatorSubscriptionStatus.Unsubscribing;

                await Task.WhenAll(Subscriptions.Select(subscription => subscription.UnsubscribeAsync(cancellationToken)));

                SubscriptionStatus = ThunderPropagatorSubscriptionStatus.Unsubscribed;
            }
            catch (Exception exception)
            {
                SubscriptionStatus = ThunderPropagatorSubscriptionStatus.HasError;
                _logger.Log(LogLevel.Error, exception, "An error occured while unsubscribing subscription");
            }
        }
    }
}