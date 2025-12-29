using ThunderPropagator.Clients.DotNet.Connections.Quic;
using ThunderPropagator.Clients.DotNet.Infrastructure.Loggers;
using ThunderPropagator.Clients.DotNet.Models.Enums;

namespace ThunderPropagator.Clients.DotNet.Clients
{
    public
#if !DEBUG
        sealed
#endif
        class ThunderPropagatorQuicClient : ThunderPropagatorClient
    {
        public ThunderPropagatorQuicClient(ThunderPropagatorQuicConnectionConfiguration configuration, ILoggerProvider loggerProvider)
            : base(ThunderPropagatorProtocolType.Quic, configuration, loggerProvider)
        {
        }
    }
}