using ThunderPropagator.Clients.DotNet.Connections.InfiniteDataStream;
using Xunit;

namespace ThunderPropagator.UnitTests.Connections.InfiniteDataStream
{
    public class ThunderPropagatorInfiniteDataStreamConnectionConfigurationTests
    {
        [Fact]
        public void Constructor_InitializesWithDefaultValues()
        {
            // Act
            var config = new ThunderPropagatorInfiniteDataStreamConnectionConfiguration();

            // Assert
            Assert.NotNull(config);
        }

        [Fact]
        public void Uri_CanBeSetAndRetrieved()
        {
            // Arrange
            var config = new ThunderPropagatorInfiniteDataStreamConnectionConfiguration();
            var expectedUri = "ids://localhost:8080";

            // Act
            config.Uri = expectedUri;

            // Assert
            Assert.Equal(expectedUri, config.Uri);
        }

        [Fact]
        public void Uri_WithHttpsScheme_IsAccepted()
        {
            // Arrange
            var config = new ThunderPropagatorInfiniteDataStreamConnectionConfiguration();
            var expectedUri = "https://localhost:8080/stream";

            // Act
            config.Uri = expectedUri;

            // Assert
            Assert.Equal(expectedUri, config.Uri);
        }

        [Fact]
        public void Configuration_SupportsFluentConfiguration()
        {
            // Arrange & Act
            var config = new ThunderPropagatorInfiniteDataStreamConnectionConfiguration
            {
                Uri = "ids://stream.example.com:7070"
            };

            // Assert
            Assert.Equal("ids://stream.example.com:7070", config.Uri);
        }
    }
}
