using NSubstitute;
using ThunderPropagator.Clients.DotNet.Clients;
using ThunderPropagator.Clients.DotNet.Connections.WebSocket;
using ThunderPropagator.Clients.DotNet.Infrastructure.Loggers;
using ThunderPropagator.Clients.DotNet.Models.Enums;
using Xunit;

namespace ThunderPropagator.UnitTests.Clients
{
    public class ThunderPropagatorWebSocketClientTests
    {
        private readonly ILoggerProvider _mockLoggerProvider;
        private readonly ILogger _mockLogger;

        public ThunderPropagatorWebSocketClientTests()
        {
            _mockLoggerProvider = Substitute.For<ILoggerProvider>();
            _mockLogger = Substitute.For<ILogger>();
            _mockLoggerProvider.CreateLogger(Arg.Any<string>()).Returns(_mockLogger);
        }

        [Fact]
        public void Constructor_InitializesWithWebSocketProtocol()
        {
            // Arrange
            var config = new ThunderPropagatorWebSocketConnectionConfiguration
            {
                Uri = "wss://localhost:8080"
            };

            // Act
            var client = new ThunderPropagatorWebSocketClient(config, _mockLoggerProvider);

            // Assert
            Assert.Equal(ThunderPropagatorProtocolType.WebSocket, client.ConnectionProtocol);
        }

        [Fact]
        public void Constructor_WithValidConfiguration_CreatesConnection()
        {
            // Arrange
            var config = new ThunderPropagatorWebSocketConnectionConfiguration
            {
                Uri = "wss://localhost:8080"
            };

            // Act
            var client = new ThunderPropagatorWebSocketClient(config, _mockLoggerProvider);

            // Assert
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
            var client = new ThunderPropagatorWebSocketClient(config, _mockLoggerProvider);

            // Assert
            _mockLoggerProvider.Received().CreateLogger(Arg.Any<string>());
        }
    }
}
