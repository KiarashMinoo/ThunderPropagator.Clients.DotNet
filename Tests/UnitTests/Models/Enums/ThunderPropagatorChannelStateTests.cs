using ThunderPropagator.Clients.DotNet.Models.Enums;
using Xunit;

namespace ThunderPropagator.UnitTests.Models.Enums
{
    public class ThunderPropagatorChannelStateTests
    {
        [Fact]
        public void ChannelState_HasReadyValue()
        {
            // Act
            var state = ThunderPropagatorChannelState.Ready;

            // Assert
            Assert.Equal(ThunderPropagatorChannelState.Ready, state);
        }

        [Fact]
        public void ChannelState_HasRequestingMetadataValue()
        {
            // Act
            var state = ThunderPropagatorChannelState.RequestingMetadata;

            // Assert
            Assert.Equal(ThunderPropagatorChannelState.RequestingMetadata, state);
        }

        [Fact]
        public void ChannelState_HasHasMetadataValue()
        {
            // Act
            var state = ThunderPropagatorChannelState.HasMetadata;

            // Assert
            Assert.Equal(ThunderPropagatorChannelState.HasMetadata, state);
        }

        [Fact]
        public void ChannelState_HasHasErrorValue()
        {
            // Act
            var state = ThunderPropagatorChannelState.HasError;

            // Assert
            Assert.Equal(ThunderPropagatorChannelState.HasError, state);
        }

        [Fact]
        public void ChannelState_HasFourValues()
        {
            // Act
            var values = System.Enum.GetValues(typeof(ThunderPropagatorChannelState));

            // Assert
            Assert.Equal(4, values.Length);
        }

        [Fact]
        public void ChannelState_CanBeCompared()
        {
            // Arrange
            var state1 = ThunderPropagatorChannelState.Ready;
            var state2 = ThunderPropagatorChannelState.Ready;

            // Act & Assert
            Assert.Equal(state1, state2);
        }
    }
}
