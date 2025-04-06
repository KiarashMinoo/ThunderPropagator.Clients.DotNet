using RapidStreamer.Clients.DotNet.Infrastructure.Connections;

namespace RapidStreamer.Clients.DotNet.Connections.WebSocket
{
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerWebSocketConnectionConfiguration : AbstractRapidStreamerConfiguration
    {
        public int BufferSize
        {
            get => Get(1024 * 4);
            set => Set(value);
        }
    }
}