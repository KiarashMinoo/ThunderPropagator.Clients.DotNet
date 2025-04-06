using RapidStreamer.Clients.DotNet.Infrastructure.Requests;

namespace RapidStreamer.Clients.DotNet.Models.Requests
{
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerPingRequest : RapidStreamerRequestBase
    {
        public const string PingingKey = "Ping";

        public RapidStreamerPingRequest(string requestId, string channelName) : base(requestId, channelName, PingingKey)
        {
        }
    }
}