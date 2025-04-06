using RapidStreamer.Clients.DotNet.Connections.WebSocket;
using RapidStreamer.Clients.DotNet.Infrastructure.Loggers;
using RapidStreamer.Clients.DotNet.Models.Enums;

namespace RapidStreamer.Clients.DotNet.Clients
{
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerWebSocketClient : RapidStreamerClient
    {
        public RapidStreamerWebSocketClient(RapidStreamerWebSocketConnectionConfiguration configuration, ILoggerProvider loggerProvider)
            : base(RapidStreamerProtocolType.WebSocket, configuration, loggerProvider)
        {
        }
    }
}