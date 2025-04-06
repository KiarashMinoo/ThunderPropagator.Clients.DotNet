using RapidStreamer.Clients.DotNet.Models.Connections;
using RapidStreamer.Clients.DotNet.Models.Enums;

namespace RapidStreamer.Clients.DotNet.Infrastructure.Connections
{
    public interface IRapidStreamerConnection : IDisposable,
        IAsyncDisposable
    {
        string ConnectionId { get; }
        RapidStreamerConnectionState ConnectionState { get; }
        RapidStreamerConnectionResponse ConnectionInfo { get; }

        event RapidStreamerConnectionStateChangedEventHandler? ConnectionStateChanged;
        event RapidStreamerMessageReceivedEventHandler? MessageReceived;

        Task ConnectAsync(CancellationToken cancellationToken = default);
        Task DisconnectAsync(CancellationToken cancellationToken = default);

        internal Task SendAsync(string message, CancellationToken cancellationToken = default);
    }
}