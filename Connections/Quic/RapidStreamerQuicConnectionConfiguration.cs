using RapidStreamer.Clients.DotNet.Infrastructure.Connections;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Quic;

namespace RapidStreamer.Clients.DotNet.Connections.Quic
{
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerQuicConnectionConfiguration : AbstractRapidStreamerConfiguration
    {
        public EndPoint RemoteEndPoint
        {
            get => Get<EndPoint>()!;
            set => Set(value);
        }

        public long DefaultStreamErrorCode
        {
            get => Get(0x0A);
            set => Set(value);
        }

        public long DefaultCloseErrorCode
        {
            get => Get(0x0B);
            set => Set(value);
        }

        public int MaxInboundBidirectionalStreams
        {
            get => Get(0);
            set => Set(value);
        }

        public int MaxInboundUnidirectionalStreams
        {
            get => Get(0);
            set => Set(value);
        }

        public string[] SslProtocols
        {
            get => Get(Array.Empty<string>());
            set => Set(value);
        }

        public QuicStreamType StreamType
        {
            get => Get(QuicStreamType.Bidirectional);
            set => Set(value);
        }

        public int BufferSize
        {
            get => Get(1024 * 4);
            set => Set(value);
        }
    }
}