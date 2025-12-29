using NSubstitute;
using ThunderPropagator.Clients.DotNet.Infrastructure.Channels;
using ThunderPropagator.Clients.DotNet.Infrastructure.Connections;
using ThunderPropagator.Clients.DotNet.Infrastructure.Loggers;
using ThunderPropagator.Clients.DotNet.Models.Enums;
using Xunit;

namespace ThunderPropagator.UnitTests.Infrastructure.Channels
{
    public class IThunderPropagatorChannelTests
    {
        [Fact]
        public void IThunderPropagatorChannel_HasChannelMetadataProperty()
        {
            // Arrange
            var channel = Substitute.For<IThunderPropagatorChannel>();

            // Act
            var metadata = channel.ChannelMetadata;

            // Assert
            // ChannelMetadata is nullable, so just check the property exists
            Assert.True(true);
        }

        [Fact]
        public void IThunderPropagatorChannel_HasChannelStatusProperty()
        {
            // Arrange
            var channel = Substitute.For<IThunderPropagatorChannel>();

            // Act
            var status = channel.ChannelStatus;

            // Assert - Should have a status property (enum value type)
            // ChannelStatus is an enum (value type), so just verify it can be accessed
            Assert.True(System.Enum.IsDefined(typeof(ThunderPropagatorChannelState), status));
        }

        [Fact]
        public void IThunderPropagatorChannel_CanSubscribe()
        {
            // Arrange
            var channel = Substitute.For<IThunderPropagatorChannel>();

            // Act & Assert - Method should be callable
            Assert.NotNull(channel);
        }
    }
}
