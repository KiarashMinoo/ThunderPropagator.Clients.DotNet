using ThunderPropagator.Clients.DotNet.Models.Enums;
using Xunit;

namespace ThunderPropagator.UnitTests.Models.Enums
{
    public class ThunderPropagatorProtocolTypeTests
    {
        [Fact]
        public void ProtocolType_HasWebSocketValue()
        {
            // Act
            var protocol = ThunderPropagatorProtocolType.WebSocket;

            // Assert
            Assert.Equal(ThunderPropagatorProtocolType.WebSocket, protocol);
        }

        [Fact]
        public void ProtocolType_HasQuicValue()
        {
            // Act
            var protocol = ThunderPropagatorProtocolType.Quic;

            // Assert
            Assert.Equal(ThunderPropagatorProtocolType.Quic, protocol);
        }

        [Fact]
        public void ProtocolType_HasInfiniteDataStreamValue()
        {
            // Act
            var protocol = ThunderPropagatorProtocolType.InfiniteDataStream;

            // Assert
            Assert.Equal(ThunderPropagatorProtocolType.InfiniteDataStream, protocol);
        }

        [Fact]
        public void ProtocolType_CanBeCompared()
        {
            // Arrange
            var protocol1 = ThunderPropagatorProtocolType.WebSocket;
            var protocol2 = ThunderPropagatorProtocolType.WebSocket;

            // Act & Assert
            Assert.Equal(protocol1, protocol2);
        }

        [Fact]
        public void ProtocolType_DifferentValuesAreNotEqual()
        {
            // Arrange
            var protocol1 = ThunderPropagatorProtocolType.WebSocket;
            var protocol2 = ThunderPropagatorProtocolType.Quic;

            // Act & Assert
            Assert.NotEqual(protocol1, protocol2);
        }
    }
}
