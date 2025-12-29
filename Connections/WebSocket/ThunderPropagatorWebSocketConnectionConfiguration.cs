using ThunderPropagator.Clients.DotNet.Infrastructure.Connections;

namespace ThunderPropagator.Clients.DotNet.Connections.WebSocket
{
    public
#if !DEBUG
        sealed
#endif
        class ThunderPropagatorWebSocketConnectionConfiguration : AbstractThunderPropagatorConfiguration
    {
        public int BufferSize
        {
            get => Get(1024 * 4);
            set => Set(value);
        }
    }
}