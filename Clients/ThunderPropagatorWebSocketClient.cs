using ThunderPropagator.Clients.DotNet.Connections.WebSocket;
using ThunderPropagator.Clients.DotNet.Infrastructure.Loggers;
using ThunderPropagator.Clients.DotNet.Models.Enums;

namespace ThunderPropagator.Clients.DotNet.Clients
{
    public
#if !DEBUG
        sealed
#endif
        class ThunderPropagatorWebSocketClient : ThunderPropagatorClient
    {
        public ThunderPropagatorWebSocketClient(ThunderPropagatorWebSocketConnectionConfiguration configuration, ILoggerProvider loggerProvider)
            : base(ThunderPropagatorProtocolType.WebSocket, configuration, loggerProvider)
        {
        }
    }
}