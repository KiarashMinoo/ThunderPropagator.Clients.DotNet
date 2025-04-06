using RapidStreamer.Clients.DotNet.Infrastructure.Requests;

namespace RapidStreamer.Clients.DotNet.Models.Requests
{
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerMetadataRequest : RapidStreamerRequestBase
    {
        public const string RequestMetadataKey = "RequestMetadata";

        public RapidStreamerMetadataRequest(string requestId, string channelName) : base(requestId, channelName, RequestMetadataKey)
        {
        }
    }
}