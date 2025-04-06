using RapidStreamer.Clients.DotNet.Infrastructure.Requests;
using RapidStreamer.Clients.DotNet.Models.Enums;
using RapidStreamer.Clients.DotNet.Models.Subscriptions;

namespace RapidStreamer.Clients.DotNet.Models.Requests
{
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerSubscriptionRequest : RapidStreamerRequestBase
    {
        public const string SubscribeRoutingKey = "Subscribe";

        internal RapidStreamerSubscription RapidStreamerSubscription { get; }

        public IReadOnlyCollection<IReadOnlyDictionary<string, string>> SubscribingKeys { get; }
        public IReadOnlyCollection<string> SubscribingFields { get; }
        public RapidStreamerSubscriptionMode SubscriptionMode { get; }

        internal RapidStreamerSubscriptionRequest(RapidStreamerSubscription rapidStreamerSubscription,
            string requestId,
            string channelName,
            IReadOnlyCollection<IReadOnlyDictionary<string, string>> subscribingKeys,
            IReadOnlyCollection<string> subscribingFields,
            RapidStreamerSubscriptionMode subscriptionMode = RapidStreamerSubscriptionMode.Full)
            : base(requestId, channelName, SubscribeRoutingKey)
        {
            RapidStreamerSubscription = rapidStreamerSubscription;
            SubscribingKeys = subscribingKeys;
            SubscribingFields = subscribingFields;
            SubscriptionMode = subscriptionMode;
        }

        internal void SetSubscribed() => RapidStreamerSubscription.SubscriptionStatus = RapidStreamerSubscriptionStatus.Subscribed;
    }
}