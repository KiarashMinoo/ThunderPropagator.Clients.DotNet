using ThunderPropagator.Clients.DotNet.Connections.InfiniteDataStream;
using ThunderPropagator.Clients.DotNet.Infrastructure.Loggers;
using ThunderPropagator.Clients.DotNet.Models.Enums;

namespace ThunderPropagator.Clients.DotNet.Clients
{
    public
#if !DEBUG
        sealed
#endif
        class ThunderPropagatorInfiniteDataStreamClient : ThunderPropagatorClient
    {
        public ThunderPropagatorInfiniteDataStreamClient(ThunderPropagatorInfiniteDataStreamConnectionConfiguration configuration, ILoggerProvider loggerProvider)
            : base(ThunderPropagatorProtocolType.InfiniteDataStream, configuration, loggerProvider)
        {
        }
    }
}