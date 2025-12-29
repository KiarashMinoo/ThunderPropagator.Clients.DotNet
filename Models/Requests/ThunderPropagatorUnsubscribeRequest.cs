using ThunderPropagator.Clients.DotNet.Infrastructure.Requests;
using ThunderPropagator.Clients.DotNet.Models.Enums;
using ThunderPropagator.Clients.DotNet.Models.Subscriptions;

namespace ThunderPropagator.Clients.DotNet.Models.Requests
{
    public
#if !DEBUG
        sealed
#endif
        class ThunderPropagatorUnsubscribeRequest : ThunderPropagatorRequestBase
    {
        public const string UnsubscribeRoutingKey = "Unsubscribe";

        internal ThunderPropagatorSubscription ThunderPropagatorSubscription { get; }

        public IReadOnlyCollection<IReadOnlyDictionary<string, string>> SubscribedKeys { get; }

        internal ThunderPropagatorUnsubscribeRequest(ThunderPropagatorSubscription thunderPropagatorSubscription,
            string requestId,
            string channelName,
            IReadOnlyCollection<IReadOnlyDictionary<string, string>> subscribedKeys)
            : base(requestId, channelName, UnsubscribeRoutingKey)
        {
            ThunderPropagatorSubscription = thunderPropagatorSubscription;
            SubscribedKeys = subscribedKeys;
        }

        internal void SetUnsubscribed() => ThunderPropagatorSubscription.SubscriptionStatus = ThunderPropagatorSubscriptionStatus.Unsubscribed;
    }
}