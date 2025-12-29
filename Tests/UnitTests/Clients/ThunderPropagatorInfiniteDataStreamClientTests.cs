using NSubstitute;
using ThunderPropagator.Clients.DotNet.Clients;
using ThunderPropagator.Clients.DotNet.Connections.InfiniteDataStream;
using ThunderPropagator.Clients.DotNet.Infrastructure.Loggers;
using ThunderPropagator.Clients.DotNet.Models.Enums;
using Xunit;

namespace ThunderPropagator.UnitTests.Clients
{
    public class ThunderPropagatorInfiniteDataStreamClientTests
    {
        private readonly ILoggerProvider _mockLoggerProvider;
        private readonly ILogger _mockLogger;

        public ThunderPropagatorInfiniteDataStreamClientTests()
        {
            _mockLoggerProvider = Substitute.For<ILoggerProvider>();
            _mockLogger = Substitute.For<ILogger>();
            _mockLoggerProvider.CreateLogger(Arg.Any<string>()).Returns(_mockLogger);
        }

        [Fact]
        public void Constructor_InitializesWithInfiniteDataStreamProtocol()
        {
            // Arrange
            var config = new ThunderPropagatorInfiniteDataStreamConnectionConfiguration
            {
                Uri = "ids://localhost:8080"
            };

            // Act
            var client = new ThunderPropagatorInfiniteDataStreamClient(config, _mockLoggerProvider);

            // Assert
            Assert.Equal(ThunderPropagatorProtocolType.InfiniteDataStream, client.ConnectionProtocol);
        }

        [Fact]
        public void Constructor_WithValidConfiguration_CreatesConnection()
        {
            // Arrange
            var config = new ThunderPropagatorInfiniteDataStreamConnectionConfiguration
            {
                Uri = "ids://localhost:8080"
            };

            // Act
            var client = new ThunderPropagatorInfiniteDataStreamClient(config, _mockLoggerProvider);

            // Assert
            Assert.NotNull(client.ThunderPropagatorConnection);
        }

        [Fact]
        public void Constructor_CreatesLogger()
        {
            // Arrange
            var config = new ThunderPropagatorInfiniteDataStreamConnectionConfiguration
            {
                Uri = "ids://localhost:8080"
            };

            // Act
            var client = new ThunderPropagatorInfiniteDataStreamClient(config, _mockLoggerProvider);

            // Assert
            _mockLoggerProvider.Received().CreateLogger(Arg.Any<string>());
        }
    }
}
