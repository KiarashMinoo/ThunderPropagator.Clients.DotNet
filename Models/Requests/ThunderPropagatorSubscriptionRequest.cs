using ThunderPropagator.Clients.DotNet.Infrastructure.Requests;
using ThunderPropagator.Clients.DotNet.Models.Enums;
using ThunderPropagator.Clients.DotNet.Models.Subscriptions;

namespace ThunderPropagator.Clients.DotNet.Models.Requests
{
    public
#if !DEBUG
        sealed
#endif
        class ThunderPropagatorSubscriptionRequest : ThunderPropagatorRequestBase
    {
        public const string SubscribeRoutingKey = "Subscribe";

        internal ThunderPropagatorSubscription ThunderPropagatorSubscription { get; }

        public IReadOnlyCollection<IReadOnlyDictionary<string, string>> SubscribingKeys { get; }
        public IReadOnlyCollection<string> SubscribingFields { get; }
        public ThunderPropagatorSubscriptionMode SubscriptionMode { get; }

        internal ThunderPropagatorSubscriptionRequest(ThunderPropagatorSubscription thunderPropagatorSubscription,
            string requestId,
            string channelName,
            IReadOnlyCollection<IReadOnlyDictionary<string, string>> subscribingKeys,
            IReadOnlyCollection<string> subscribingFields,
            ThunderPropagatorSubscriptionMode subscriptionMode = ThunderPropagatorSubscriptionMode.Full)
            : base(requestId, channelName, SubscribeRoutingKey)
        {
            ThunderPropagatorSubscription = thunderPropagatorSubscription;
            SubscribingKeys = subscribingKeys;
            SubscribingFields = subscribingFields;
            SubscriptionMode = subscriptionMode;
        }

        internal void SetSubscribed() => ThunderPropagatorSubscription.SubscriptionStatus = ThunderPropagatorSubscriptionStatus.Subscribed;
    }
}