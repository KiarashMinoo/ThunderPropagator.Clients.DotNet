using RapidStreamer.Clients.DotNet.Connections.Quic;
using RapidStreamer.Clients.DotNet.Infrastructure.Loggers;
using RapidStreamer.Clients.DotNet.Models.Enums;

namespace RapidStreamer.Clients.DotNet.Clients
{
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerQuicClient : RapidStreamerClient
    {
        public RapidStreamerQuicClient(RapidStreamerQuicConnectionConfiguration configuration, ILoggerProvider loggerProvider)
            : base(RapidStreamerProtocolType.Quic, configuration, loggerProvider)
        {
        }
    }
}