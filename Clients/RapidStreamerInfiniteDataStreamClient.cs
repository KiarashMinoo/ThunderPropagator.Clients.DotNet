using RapidStreamer.Clients.DotNet.Connections.InfiniteDataStream;
using RapidStreamer.Clients.DotNet.Infrastructure.Loggers;
using RapidStreamer.Clients.DotNet.Models.Enums;

namespace RapidStreamer.Clients.DotNet.Clients
{
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerInfiniteDataStreamClient : RapidStreamerClient
    {
        public RapidStreamerInfiniteDataStreamClient(RapidStreamerInfiniteDataStreamConnectionConfiguration configuration, ILoggerProvider loggerProvider)
            : base(RapidStreamerProtocolType.InfiniteDataStream, configuration, loggerProvider)
        {
        }
    }
}