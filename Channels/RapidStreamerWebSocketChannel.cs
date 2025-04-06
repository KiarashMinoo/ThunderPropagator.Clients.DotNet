using RapidStreamer.Clients.DotNet.Infrastructure.Channels;
using RapidStreamer.Clients.DotNet.Infrastructure.Connections;
using RapidStreamer.Clients.DotNet.Infrastructure.Loggers;

namespace RapidStreamer.Clients.DotNet.Channels
{
    internal
#if !DEBUG
        sealed
#endif
        class RapidStreamerWebSocketChannel : AbstractRapidStreamerChannel
    {
        public RapidStreamerWebSocketChannel(string name, IRapidStreamerConnection rapidStreamerConnection, ILoggerProvider loggerProvider)
            : base(name, rapidStreamerConnection, loggerProvider)
        {
        }
    }
}