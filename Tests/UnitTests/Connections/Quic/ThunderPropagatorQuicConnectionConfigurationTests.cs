using ThunderPropagator.Clients.DotNet.Connections.Quic;
using Xunit;

namespace ThunderPropagator.UnitTests.Connections.Quic
{
    public class ThunderPropagatorQuicConnectionConfigurationTests
    {
        [Fact]
        public void Constructor_InitializesWithDefaultValues()
        {
            // Act
            var config = new ThunderPropagatorQuicConnectionConfiguration();

            // Assert
            Assert.NotNull(config);
        }

        [Fact]
        public void Uri_CanBeSetAndRetrieved()
        {
            // Arrange
            var config = new ThunderPropagatorQuicConnectionConfiguration();
            var expectedUri = "quic://localhost:8080";

            // Act
            config.Uri = expectedUri;

            // Assert
            Assert.Equal(expectedUri, config.Uri);
        }

        [Fact]
        public void Uri_WithHttpsScheme_IsAccepted()
        {
            // Arrange
            var config = new ThunderPropagatorQuicConnectionConfiguration();
            var expectedUri = "https://localhost:8080/channel";

            // Act
            config.Uri = expectedUri;

            // Assert
            Assert.Equal(expectedUri, config.Uri);
        }

        [Fact]
        public void Configuration_SupportsFluentConfiguration()
        {
            // Arrange & Act
            var config = new ThunderPropagatorQuicConnectionConfiguration
            {
                Uri = "quic://example.com:9090"
            };

            // Assert
            Assert.Equal("quic://example.com:9090", config.Uri);
        }
    }
}
