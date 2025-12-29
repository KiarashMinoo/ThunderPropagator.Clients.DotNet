namespace ThunderPropagator.Clients.DotNet.Infrastructure.Requests
{
    public
#if !DEBUG
        sealed
#endif
        class ThunderPropagatorRequestRoute
    {
        public string Channel { get; }
        public string RequestType { get; }

        public ThunderPropagatorRequestRoute(string channel, string requestType)
        {
            Channel = channel;
            RequestType = requestType;
        }
    }
}