using RapidStreamer.Clients.DotNet.Models.Enums;

namespace RapidStreamer.Clients.DotNet.Models.Subscriptions
{
    public interface IRapidStreamerSubscriptionOperational
    {
        RapidStreamerSubscriptionMode SubscriptionMode { get; }
        RapidStreamerSubscriptionStatus SubscriptionStatus { get; }

        event RapidStreamerSubscriptionStatusChangedHandler? StatusChanged;
        event RapidStreamerFieldUpdatedEventHandler? FieldUpdated;
        event RapidStreamerTableUpdatedEventHandler? TableUpdated;

        Task SubscribeAsync(CancellationToken cancellationToken = default);
        Task UnsubscribeAsync(CancellationToken cancellationToken = default);
    }
}