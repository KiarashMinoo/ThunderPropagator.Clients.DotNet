using ThunderPropagator.Clients.DotNet.Models.Enums;

namespace ThunderPropagator.Clients.DotNet.Models.Subscriptions
{
    public interface IThunderPropagatorSubscriptionOperational
    {
        ThunderPropagatorSubscriptionMode SubscriptionMode { get; }
        ThunderPropagatorSubscriptionStatus SubscriptionStatus { get; }

        event ThunderPropagatorSubscriptionStatusChangedHandler? StatusChanged;
        event ThunderPropagatorFieldUpdatedEventHandler? FieldUpdated;
        event ThunderPropagatorTableUpdatedEventHandler? TableUpdated;

        Task SubscribeAsync(CancellationToken cancellationToken = default);
        Task UnsubscribeAsync(CancellationToken cancellationToken = default);
    }
}