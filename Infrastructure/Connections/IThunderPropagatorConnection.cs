using ThunderPropagator.Clients.DotNet.Models.Connections;
using ThunderPropagator.Clients.DotNet.Models.Enums;

namespace ThunderPropagator.Clients.DotNet.Infrastructure.Connections
{
    public interface IThunderPropagatorConnection : IDisposable,
        IAsyncDisposable
    {
        string ConnectionId { get; }
        ThunderPropagatorConnectionState ConnectionState { get; }
        ThunderPropagatorConnectionResponse ConnectionInfo { get; }

        event ThunderPropagatorConnectionStateChangedEventHandler? ConnectionStateChanged;
        event ThunderPropagatorMessageReceivedEventHandler? MessageReceived;

        Task ConnectAsync(CancellationToken cancellationToken = default);
        Task DisconnectAsync(CancellationToken cancellationToken = default);

        internal Task SendAsync(string message, CancellationToken cancellationToken = default);
    }
}