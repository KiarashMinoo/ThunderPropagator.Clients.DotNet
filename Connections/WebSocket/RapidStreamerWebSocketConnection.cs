using RapidStreamer.BuildingBlocks.Application.Helpers;
using RapidStreamer.Clients.DotNet.Infrastructure.Connections;
using RapidStreamer.Clients.DotNet.Infrastructure.Loggers;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;

namespace RapidStreamer.Clients.DotNet.Connections.WebSocket
{
    internal
#if !DEBUG
        sealed
#endif
        class RapidStreamerWebSocketConnection : AbstractRapidStreamerConnection<RapidStreamerWebSocketConnectionConfiguration>
    {
        private readonly ClientWebSocket _clientWebSocket;

        public RapidStreamerWebSocketConnection(RapidStreamerWebSocketConnectionConfiguration connectionConfiguration, ILoggerProvider loggerProvider)
            : base(connectionConfiguration, loggerProvider)
        {
            _clientWebSocket = new ClientWebSocket();
        }

        protected override Task InternalConnectAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ConnectionConfiguration.Uri))
                throw new InvalidOperationException($"{nameof(Uri)} has not configured");

            return _clientWebSocket.ConnectAsync(new Uri(ConnectionConfiguration.Uri), cancellationToken);
        }

        protected override Task InternalDisconnectAsync(CancellationToken cancellationToken = default)
        {
            return _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "CLOSED_BY_CLIENT", cancellationToken);
        }

        protected override async ValueTask<string> ReceiveAsync(CancellationToken cancellationToken = default)
        {
            if (_clientWebSocket.State != WebSocketState.Open)
                throw new InvalidOperationException("Socket is not open");

            List<byte> bytes = [];

            var buffer = new ArraySegment<byte>(new byte[ConnectionConfiguration.BufferSize]);
            WebSocketReceiveResult result;

            do
            {
                result = await _clientWebSocket.ReceiveAsync(buffer, cancellationToken);
                bytes.AddRange(buffer.ToArray());
            } while (!result.EndOfMessage);

            return Convert();

            string Convert()
            {
                var span = CollectionsMarshal.AsSpan(bytes);
                return Encoding.UTF8.GetString(span);
            }
        }

        protected override Task InternalSendAsync(string message, CancellationToken cancellationToken)
            => _clientWebSocket.SendAsync(message.ToByteArray(), WebSocketMessageType.Text, true, cancellationToken);

        protected override void DisposeManagedResources()
        {
            _clientWebSocket.Dispose();
        }
    }
}