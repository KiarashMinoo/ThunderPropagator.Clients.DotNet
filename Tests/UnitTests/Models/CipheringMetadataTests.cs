using ThunderPropagator.Clients.DotNet.Models;
using Xunit;

namespace ThunderPropagator.UnitTests.Models
{
    public class CipheringMetadataTests
    {
        [Fact]
        public void Constructor_RequiresKeyProperty()
        {
            // Act
            var metadata = new CipheringMetadata { Key = "test-key" };

            // Assert
            Assert.NotNull(metadata);
            Assert.Equal("test-key", metadata.Key);
        }

        [Fact]
        public void KeySize_HasDefaultValue()
        {
            // Arrange & Act
            var metadata = new CipheringMetadata { Key = "test-key" };

            // Assert
            Assert.Equal(512, metadata.KeySize);
        }

        [Fact]
        public void Key_CanBeRetrieved()
        {
            // Arrange
            var expectedKey = "test-public-key";
            var metadata = new CipheringMetadata { Key = expectedKey };

            // Assert
            Assert.Equal(expectedKey, metadata.Key);
        }

        [Fact]
        public void Metadata_SupportsFluentConfiguration()
        {
            // Arrange & Act
            var metadata = new CipheringMetadata
            {
                KeySize = 1024,
                Key = "public-key-data"
            };

            // Assert
            Assert.Equal(1024, metadata.KeySize);
            Assert.Equal("public-key-data", metadata.Key);
        }
    }
}
