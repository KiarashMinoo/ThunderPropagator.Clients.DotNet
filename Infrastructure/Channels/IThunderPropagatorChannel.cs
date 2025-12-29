using ThunderPropagator.Clients.DotNet.Infrastructure.Responses;
using ThunderPropagator.Clients.DotNet.Models;
using ThunderPropagator.Clients.DotNet.Models.Enums;
using ThunderPropagator.Clients.DotNet.Models.Metadata;
using ThunderPropagator.Clients.DotNet.Models.Requests;
using ThunderPropagator.Clients.DotNet.Models.Subscriptions;

namespace ThunderPropagator.Clients.DotNet.Infrastructure.Channels
{
    public interface IThunderPropagatorChannel
    {
        ThunderPropagatorChannelMetadata? ChannelMetadata { get; }
        ThunderPropagatorChannelState ChannelStatus { get; }
        event ThunderPropagatorChannelStatusChangedEventHandler? ChannelStatusChanged;
        event ThunderPropagatorMetadataUpdatedEventHandler? MetadataUpdated;
        event ThunderPropagatorChannelReceivedMessageEventHandler? ReceivedMessage;

        void SetToken(string token);
        void SetAuthentication(string username, string password, CipheringMetadata cipheringMetadata);
        void SetMessageCipheringMetadata(CipheringMetadata cipheringMetadata);

        internal void HandleReceivedResponse(ThunderPropagatorResponseBase response);
        internal Task HandleReceivedMessageAsync(string message, CancellationToken cancellationToken = default);

        Task RequestPingAsync(CancellationToken cancellationToken = default);
        internal void SetChannelMetadata(ThunderPropagatorChannelMetadata channelMetadata);
        internal Task RequestChannelMetadataAsync(bool awaitable, CancellationToken cancellationToken = default);
        Task RequestChannelMetadataAsync(CancellationToken cancellationToken = default);
        internal Task RequestSubscriptionAsync(ThunderPropagatorSubscriptionRequest request, CancellationToken cancellationToken = default);
        internal Task RequestUnsubscribeAsync(ThunderPropagatorUnsubscribeRequest request, CancellationToken cancellationToken = default);


        IThunderPropagatorSubscriptionOperational CreateSubscription(IReadOnlyDictionary<string, string> subscribingKeys,
            IReadOnlyCollection<string> subscribingFields,
            ThunderPropagatorSubscriptionMode subscriptionMode = ThunderPropagatorSubscriptionMode.Full);

        IThunderPropagatorSubscriptionOperational CreateSubscription(IReadOnlyCollection<IReadOnlyDictionary<string, string>> subscribingKeys,
            IReadOnlyCollection<string> subscribingFields,
            ThunderPropagatorSubscriptionMode subscriptionMode = ThunderPropagatorSubscriptionMode.Full);
    }
}