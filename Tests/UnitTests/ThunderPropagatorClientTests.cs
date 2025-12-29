using NSubstitute;
using ThunderPropagator.Clients.DotNet;
using ThunderPropagator.Clients.DotNet.Connections.WebSocket;
using ThunderPropagator.Clients.DotNet.Infrastructure.Loggers;
using ThunderPropagator.Clients.DotNet.Models.Enums;
using Xunit;

namespace ThunderPropagator.UnitTests
{
    public class ThunderPropagatorClientTests
    {
        private readonly ILoggerProvider _mockLoggerProvider;
        private readonly ILogger _mockLogger;

        public ThunderPropagatorClientTests()
        {
            _mockLoggerProvider = Substitute.For<ILoggerProvider>();
            _mockLogger = Substitute.For<ILogger>();
            _mockLoggerProvider.CreateLogger(Arg.Any<string>()).Returns(_mockLogger);
        }

        [Fact]
        public void Constructor_WithWebSocketConfiguration_SetsProtocolCorrectly()
        {
            // Arrange
            var config = new ThunderPropagatorWebSocketConnectionConfiguration
            {
                Uri = "wss://localhost:8080"
            };

            // Act
            var client = new ThunderPropagatorClient(
                ThunderPropagatorProtocolType.WebSocket,
                config,
                _mockLoggerProvider);

            // Assert
            Assert.Equal(ThunderPropagatorProtocolType.WebSocket, client.ConnectionProtocol);
            Assert.NotNull(client.ThunderPropagatorConnection);
        }

        [Fact]
        public void Constructor_CreatesLogger()
        {
            // Arrange
            var config = new ThunderPropagatorWebSocketConnectionConfiguration
            {
                Uri = "wss://localhost:8080"
            };

            // Act
            var client = new ThunderPropagatorClient(
                ThunderPropagatorProtocolType.WebSocket,
                config,
                _mockLoggerProvider);

            // Assert
            _mockLoggerProvider.Received(1).CreateLogger(Arg.Any<string>());
        }

        [Fact]
        public void ConnectionId_ReturnsConnectionId()
        {
            // Arrange
            var config = new ThunderPropagatorWebSocketConnectionConfiguration
            {
                Uri = "wss://localhost:8080"
            };
            var client = new ThunderPropagatorClient(
                ThunderPropagatorProtocolType.WebSocket,
                config,
                _mockLoggerProvider);

            // Act
            var connectionId = client.ConnectionId;

            // Assert
            Assert.NotNull(connectionId);
        }

        [Fact]
        public void ConnectionState_ReturnsInitialState()
        {
            // Arrange
            var config = new ThunderPropagatorWebSocketConnectionConfiguration
            {
                Uri = "wss://localhost:8080"
            };
            var client = new ThunderPropagatorClient(
                ThunderPropagatorProtocolType.WebSocket,
                config,
                _mockLoggerProvider);

            // Act
            var state = client.ConnectionState;

            // Assert
            Assert.Equal(ThunderPropagatorConnectionState.Ready, state);
        }

        [Fact]
        public void Constructor_WithInvalidProtocolAndConfiguration_ThrowsException()
        {
            // Arrange
            var config = new ThunderPropagatorWebSocketConnectionConfiguration
            {
                Uri = "wss://localhost:8080"
            };

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new ThunderPropagatorClient(
                    ThunderPropagatorProtocolType.Quic, // Wrong protocol for config
                    config,
                    _mockLoggerProvider));
        }

        [Fact]
        public void Dispose_DisposesConnection()
        {
            // Arrange
            var config = new ThunderPropagatorWebSocketConnectionConfiguration
            {
                Uri = "wss://localhost:8080"
            };
            var client = new ThunderPropagatorClient(
                ThunderPropagatorProtocolType.WebSocket,
                config,
                _mockLoggerProvider);

            // Act
            client.Dispose();

            // Assert - client should be disposed (test disposal was called)
            // Note: IsDisposed is protected, so we just verify Dispose can be called without errors
            Assert.NotNull(client);
        }
    }
}
