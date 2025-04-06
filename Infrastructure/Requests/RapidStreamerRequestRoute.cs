namespace RapidStreamer.Clients.DotNet.Infrastructure.Requests
{
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerRequestRoute
    {
        public string Channel { get; }
        public string RequestType { get; }

        public RapidStreamerRequestRoute(string channel, string requestType)
        {
            Channel = channel;
            RequestType = requestType;
        }
    }
}