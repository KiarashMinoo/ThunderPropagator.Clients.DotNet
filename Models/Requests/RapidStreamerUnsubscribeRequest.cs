using RapidStreamer.Clients.DotNet.Infrastructure.Requests;
using RapidStreamer.Clients.DotNet.Models.Enums;
using RapidStreamer.Clients.DotNet.Models.Subscriptions;

namespace RapidStreamer.Clients.DotNet.Models.Requests
{
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerUnsubscribeRequest : RapidStreamerRequestBase
    {
        public const string UnsubscribeRoutingKey = "Unsubscribe";

        internal RapidStreamerSubscription RapidStreamerSubscription { get; }

        public IReadOnlyCollection<IReadOnlyDictionary<string, string>> SubscribedKeys { get; }

        internal RapidStreamerUnsubscribeRequest(RapidStreamerSubscription rapidStreamerSubscription,
            string requestId,
            string channelName,
            IReadOnlyCollection<IReadOnlyDictionary<string, string>> subscribedKeys)
            : base(requestId, channelName, UnsubscribeRoutingKey)
        {
            RapidStreamerSubscription = rapidStreamerSubscription;
            SubscribedKeys = subscribedKeys;
        }

        internal void SetUnsubscribed() => RapidStreamerSubscription.SubscriptionStatus = RapidStreamerSubscriptionStatus.Unsubscribed;
    }
}