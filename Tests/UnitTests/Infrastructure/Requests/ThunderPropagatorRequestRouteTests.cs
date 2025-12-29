using ThunderPropagator.Clients.DotNet.Infrastructure.Requests;
using Xunit;

namespace ThunderPropagator.UnitTests.Infrastructure.Requests
{
    public class ThunderPropagatorRequestRouteTests
    {
        [Fact]
        public void Constructor_InitializesWithParameters()
        {
            // Arrange
            var channel = "test-channel";
            var requestType = "metadata";

            // Act
            var route = new ThunderPropagatorRequestRoute(channel, requestType);

            // Assert
            Assert.NotNull(route);
            Assert.Equal(channel, route.Channel);
            Assert.Equal(requestType, route.RequestType);
        }

        [Fact]
        public void Channel_IsReadOnly()
        {
            // Arrange
            var expectedChannel = "test-channel";
            var route = new ThunderPropagatorRequestRoute(expectedChannel, "metadata");

            // Assert
            Assert.Equal(expectedChannel, route.Channel);
        }

        [Fact]
        public void RequestType_IsReadOnly()
        {
            // Arrange
            var expectedRequestType = "/api/data";
            var route = new ThunderPropagatorRequestRoute("channel", expectedRequestType);

            // Assert
            Assert.Equal(expectedRequestType, route.RequestType);
        }

        [Fact]
        public void Route_CanBeCreatedWithBothParameters()
        {
            // Arrange & Act
            var route = new ThunderPropagatorRequestRoute("my-channel", "/api/test");

            // Assert
            Assert.Equal("my-channel", route.Channel);
            Assert.Equal("/api/test", route.RequestType);
        }
    }
}
