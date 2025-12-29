using ThunderPropagator.Clients.DotNet.Models.Enums;
using Xunit;

namespace ThunderPropagator.UnitTests.Models.Enums
{
    public class ThunderPropagatorConnectionStateTests
    {
        [Fact]
        public void ConnectionState_HasReadyValue()
        {
            // Act
            var state = ThunderPropagatorConnectionState.Ready;

            // Assert
            Assert.Equal(ThunderPropagatorConnectionState.Ready, state);
        }

        [Fact]
        public void ConnectionState_HasConnectingValue()
        {
            // Act
            var state = ThunderPropagatorConnectionState.Connecting;

            // Assert
            Assert.Equal(ThunderPropagatorConnectionState.Connecting, state);
        }

        [Fact]
        public void ConnectionState_HasOpenValue()
        {
            // Act
            var state = ThunderPropagatorConnectionState.Open;

            // Assert
            Assert.Equal(ThunderPropagatorConnectionState.Open, state);
        }

        [Fact]
        public void ConnectionState_HasClosedValue()
        {
            // Act
            var state = ThunderPropagatorConnectionState.Closed;

            // Assert
            Assert.Equal(ThunderPropagatorConnectionState.Closed, state);
        }

        [Fact]
        public void ConnectionState_HasErrorValue()
        {
            // Act
            var state = ThunderPropagatorConnectionState.HasError;

            // Assert
            Assert.Equal(ThunderPropagatorConnectionState.HasError, state);
        }

        [Fact]
        public void ConnectionState_CanBeCompared()
        {
            // Arrange
            var state1 = ThunderPropagatorConnectionState.Ready;
            var state2 = ThunderPropagatorConnectionState.Ready;

            // Act & Assert
            Assert.Equal(state1, state2);
        }
    }
}
