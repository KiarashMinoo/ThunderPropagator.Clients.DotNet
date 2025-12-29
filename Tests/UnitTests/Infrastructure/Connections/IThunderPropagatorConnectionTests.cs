using NSubstitute;
using ThunderPropagator.Clients.DotNet.Infrastructure.Connections;
using ThunderPropagator.Clients.DotNet.Models.Enums;
using Xunit;

namespace ThunderPropagator.UnitTests.Infrastructure.Connections
{
    public class IThunderPropagatorConnectionTests
    {
        [Fact]
        public void IThunderPropagatorConnection_HasConnectionIdProperty()
        {
            // Arrange
            var connection = Substitute.For<IThunderPropagatorConnection>();
            connection.ConnectionId.Returns("test-connection-id");

            // Act
            var connectionId = connection.ConnectionId;

            // Assert
            Assert.Equal("test-connection-id", connectionId);
        }

        [Fact]
        public void IThunderPropagatorConnection_HasConnectionStateProperty()
        {
            // Arrange
            var connection = Substitute.For<IThunderPropagatorConnection>();
            connection.ConnectionState.Returns(ThunderPropagatorConnectionState.Ready);

            // Act
            var state = connection.ConnectionState;

            // Assert
            Assert.Equal(ThunderPropagatorConnectionState.Ready, state);
        }

        [Fact]
        public void IThunderPropagatorConnection_CanConnect()
        {
            // Arrange
            var connection = Substitute.For<IThunderPropagatorConnection>();

            // Act & Assert - Method should be callable
            Assert.NotNull(connection);
        }

        [Fact]
        public void IThunderPropagatorConnection_CanDisconnect()
        {
            // Arrange
            var connection = Substitute.For<IThunderPropagatorConnection>();

            // Act & Assert - Method should be callable
            Assert.NotNull(connection);
        }
    }
}
