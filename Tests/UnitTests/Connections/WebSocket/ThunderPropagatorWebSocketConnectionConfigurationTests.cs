using ThunderPropagator.Clients.DotNet.Connections.WebSocket;
using Xunit;

namespace ThunderPropagator.UnitTests.Connections.WebSocket
{
    public class ThunderPropagatorWebSocketConnectionConfigurationTests
    {
        [Fact]
        public void Constructor_InitializesWithDefaultValues()
        {
            // Act
            var config = new ThunderPropagatorWebSocketConnectionConfiguration();

            // Assert
            Assert.NotNull(config);
        }

        [Fact]
        public void Uri_CanBeSetAndRetrieved()
        {
            // Arrange
            var config = new ThunderPropagatorWebSocketConnectionConfiguration();
            var expectedUri = "wss://localhost:8080/channel";

            // Act
            config.Uri = expectedUri;

            // Assert
            Assert.Equal(expectedUri, config.Uri);
        }

        [Fact]
        public void Uri_WithHttpsScheme_IsAccepted()
        {
            // Arrange
            var config = new ThunderPropagatorWebSocketConnectionConfiguration();
            var expectedUri = "https://localhost:8080/channel";

            // Act
            config.Uri = expectedUri;

            // Assert
            Assert.Equal(expectedUri, config.Uri);
        }

        [Fact]
        public void Uri_WithWssScheme_IsAccepted()
        {
            // Arrange
            var config = new ThunderPropagatorWebSocketConnectionConfiguration();
            var expectedUri = "wss://secure.example.com:443";

            // Act
            config.Uri = expectedUri;

            // Assert
            Assert.Equal(expectedUri, config.Uri);
        }

        [Fact]
        public void Configuration_SupportsFluentConfiguration()
        {
            // Arrange & Act
            var config = new ThunderPropagatorWebSocketConnectionConfiguration
            {
                Uri = "wss://localhost:8080"
            };

            // Assert
            Assert.Equal("wss://localhost:8080", config.Uri);
        }
    }
}
