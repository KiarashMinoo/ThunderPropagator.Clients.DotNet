using ThunderPropagator.Clients.DotNet.Infrastructure.Requests;

namespace ThunderPropagator.Clients.DotNet.Models.Requests
{
    public
#if !DEBUG
        sealed
#endif
        class ThunderPropagatorMetadataRequest : ThunderPropagatorRequestBase
    {
        public const string RequestMetadataKey = "RequestMetadata";

        public ThunderPropagatorMetadataRequest(string requestId, string channelName) : base(requestId, channelName, RequestMetadataKey)
        {
        }
    }
}