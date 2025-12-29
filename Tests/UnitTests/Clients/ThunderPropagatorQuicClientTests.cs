using NSubstitute;
using ThunderPropagator.Clients.DotNet.Clients;
using ThunderPropagator.Clients.DotNet.Connections.Quic;
using ThunderPropagator.Clients.DotNet.Infrastructure.Loggers;
using ThunderPropagator.Clients.DotNet.Models.Enums;
using Xunit;

namespace ThunderPropagator.UnitTests.Clients
{
    public class ThunderPropagatorQuicClientTests
    {
        private readonly ILoggerProvider _mockLoggerProvider;
        private readonly ILogger _mockLogger;

        public ThunderPropagatorQuicClientTests()
        {
            _mockLoggerProvider = Substitute.For<ILoggerProvider>();
            _mockLogger = Substitute.For<ILogger>();
            _mockLoggerProvider.CreateLogger(Arg.Any<string>()).Returns(_mockLogger);
        }

        [Fact]
        public void Constructor_InitializesWithQuicProtocol()
        {
            // Arrange
            var config = new ThunderPropagatorQuicConnectionConfiguration
            {
                Uri = "quic://localhost:8080"
            };

            // Act
            var client = new ThunderPropagatorQuicClient(config, _mockLoggerProvider);

            // Assert
            Assert.Equal(ThunderPropagatorProtocolType.Quic, client.ConnectionProtocol);
        }

        [Fact]
        public void Constructor_WithValidConfiguration_CreatesConnection()
        {
            // Arrange
            var config = new ThunderPropagatorQuicConnectionConfiguration
            {
                Uri = "quic://localhost:8080"
            };

            // Act
            var client = new ThunderPropagatorQuicClient(config, _mockLoggerProvider);

            // Assert
            Assert.NotNull(client.ThunderPropagatorConnection);
        }

        [Fact]
        public void Constructor_CreatesLogger()
        {
            // Arrange
            var config = new ThunderPropagatorQuicConnectionConfiguration
            {
                Uri = "quic://localhost:8080"
            };

            // Act
            var client = new ThunderPropagatorQuicClient(config, _mockLoggerProvider);

            // Assert
            _mockLoggerProvider.Received().CreateLogger(Arg.Any<string>());
        }
    }
}
