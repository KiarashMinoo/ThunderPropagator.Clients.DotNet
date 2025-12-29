using ThunderPropagator.Clients.DotNet.Infrastructure.Requests;

namespace ThunderPropagator.Clients.DotNet.Models.Requests
{
    public
#if !DEBUG
        sealed
#endif
        class ThunderPropagatorPingRequest : ThunderPropagatorRequestBase
    {
        public const string PingingKey = "Ping";

        public ThunderPropagatorPingRequest(string requestId, string channelName) : base(requestId, channelName, PingingKey)
        {
        }
    }
}