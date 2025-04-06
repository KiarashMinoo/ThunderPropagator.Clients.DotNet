using RapidStreamer.Clients.DotNet.Infrastructure.Responses;
using RapidStreamer.Clients.DotNet.Models;
using RapidStreamer.Clients.DotNet.Models.Enums;
using RapidStreamer.Clients.DotNet.Models.Metadata;
using RapidStreamer.Clients.DotNet.Models.Requests;
using RapidStreamer.Clients.DotNet.Models.Subscriptions;

namespace RapidStreamer.Clients.DotNet.Infrastructure.Channels
{
    public interface IRapidStreamerChannel
    {
        RapidStreamerChannelMetadata? ChannelMetadata { get; }
        RapidStreamerChannelState ChannelStatus { get; }
        event RapidStreamerChannelStatusChangedEventHandler? ChannelStatusChanged;
        event RapidStreamerMetadataUpdatedEventHandler? MetadataUpdated;
        event RapidStreamerChannelReceivedMessageEventHandler? ReceivedMessage;

        void SetToken(string token);
        void SetAuthentication(string username, string password, CipheringMetadata cipheringMetadata);
        void SetMessageCipheringMetadata(CipheringMetadata cipheringMetadata);

        internal void HandleReceivedResponse(RapidStreamerResponseBase response);
        internal Task HandleReceivedMessageAsync(string message, CancellationToken cancellationToken = default);

        Task RequestPingAsync(CancellationToken cancellationToken = default);
        internal void SetChannelMetadata(RapidStreamerChannelMetadata channelMetadata);
        internal Task RequestChannelMetadataAsync(bool awaitable, CancellationToken cancellationToken = default);
        Task RequestChannelMetadataAsync(CancellationToken cancellationToken = default);
        internal Task RequestSubscriptionAsync(RapidStreamerSubscriptionRequest request, CancellationToken cancellationToken = default);
        internal Task RequestUnsubscribeAsync(RapidStreamerUnsubscribeRequest request, CancellationToken cancellationToken = default);


        IRapidStreamerSubscriptionOperational CreateSubscription(IReadOnlyDictionary<string, string> subscribingKeys,
            IReadOnlyCollection<string> subscribingFields,
            RapidStreamerSubscriptionMode subscriptionMode = RapidStreamerSubscriptionMode.Full);

        IRapidStreamerSubscriptionOperational CreateSubscription(IReadOnlyCollection<IReadOnlyDictionary<string, string>> subscribingKeys,
            IReadOnlyCollection<string> subscribingFields,
            RapidStreamerSubscriptionMode subscriptionMode = RapidStreamerSubscriptionMode.Full);
    }
}