using Ardalis.GuardClauses;
using RapidStreamer.BuildingBlocks.Application.Helpers;
using RapidStreamer.Clients.DotNet.Infrastructure.Connections;
using RapidStreamer.Clients.DotNet.Infrastructure.Loggers;
using System.Diagnostics.CodeAnalysis;
using System.Net.Quic;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Text;

namespace RapidStreamer.Clients.DotNet.Connections.Quic
{
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    internal
#if !DEBUG
        sealed
#endif
        class RapidStreamerQuicConnection : AbstractRapidStreamerConnection<RapidStreamerQuicConnectionConfiguration>
    {
        private readonly QuicClientConnectionOptions _clientConnectionOptions;
        private QuicConnection? _connection;

        public RapidStreamerQuicConnection(RapidStreamerQuicConnectionConfiguration connectionConfiguration, ILoggerProvider loggerProvider)
            : base(connectionConfiguration, loggerProvider)
        {
            if (!QuicConnection.IsSupported)
                throw new InvalidOperationException("QUIC is not supported, check for presence of libmsquic and support of TLS 1.3.");

            var sslApplicationProtocols = connectionConfiguration.SslProtocols.Select(sslProtocol => new SslApplicationProtocol(sslProtocol)).ToList();
            _clientConnectionOptions = new QuicClientConnectionOptions
            {
                RemoteEndPoint = connectionConfiguration.RemoteEndPoint,
                DefaultStreamErrorCode = connectionConfiguration.DefaultStreamErrorCode,
                DefaultCloseErrorCode = connectionConfiguration.DefaultCloseErrorCode,
                MaxInboundUnidirectionalStreams = connectionConfiguration.MaxInboundUnidirectionalStreams,
                MaxInboundBidirectionalStreams = connectionConfiguration.MaxInboundBidirectionalStreams,
                ClientAuthenticationOptions = new SslClientAuthenticationOptions { ApplicationProtocols = sslApplicationProtocols }
            };
        }

        protected override async Task InternalConnectAsync(CancellationToken cancellationToken = default)
            => _connection = await QuicConnection.ConnectAsync(_clientConnectionOptions, cancellationToken);

        protected override async Task InternalDisconnectAsync(CancellationToken cancellationToken = default)
            => await Guard.Against.Null(_connection, nameof(QuicConnection)).CloseAsync(0x0C, cancellationToken);

        protected override async ValueTask<string> ReceiveAsync(CancellationToken cancellationToken = default)
        {
            await using var inboundStream = await Guard.Against.Null(_connection, nameof(QuicConnection)).AcceptInboundStreamAsync(cancellationToken);

            if (inboundStream.Type != ConnectionConfiguration.StreamType)
                throw new InvalidOperationException($"Expected {ConnectionConfiguration.StreamType} stream, got {inboundStream.Type}");

            List<byte> bytes = [];

            var buffer = new ArraySegment<byte>(new byte[ConnectionConfiguration.BufferSize]);

            while (await inboundStream.ReadAsync(buffer, cancellationToken) > 0)
            {
                bytes.AddRange(buffer);
            }

            await inboundStream.WritesClosed;

            return Convert();

            string Convert()
            {
                var span = CollectionsMarshal.AsSpan(bytes);
                return Encoding.UTF8.GetString(span);
            }
        }

        protected override async Task InternalSendAsync(string message, CancellationToken cancellationToken)
        {
            await using var outgoingStream =
                await Guard.Against.Null(_connection, nameof(QuicConnection)).OpenOutboundStreamAsync(ConnectionConfiguration.StreamType, cancellationToken);
            await outgoingStream.WriteAsync(message.ToByteArray(), cancellationToken);
            outgoingStream.CompleteWrites();
        }

        protected override async ValueTask DisposeManagedResourcesAsync() => await Guard.Against.Null(_connection, nameof(QuicConnection)).DisposeAsync();
    }
}