using ThunderPropagator.Clients.DotNet.Infrastructure.Channels;
using ThunderPropagator.Clients.DotNet.Infrastructure.Connections;
using ThunderPropagator.Clients.DotNet.Infrastructure.Loggers;

namespace ThunderPropagator.Clients.DotNet.Channels
{
    internal
#if !DEBUG
        sealed
#endif
        class ThunderPropagatorWebSocketChannel : AbstractThunderPropagatorChannel
    {
        public ThunderPropagatorWebSocketChannel(string name, IThunderPropagatorConnection thunderPropagatorConnection, ILoggerProvider loggerProvider)
            : base(name, thunderPropagatorConnection, loggerProvider)
        {
        }
    }
}