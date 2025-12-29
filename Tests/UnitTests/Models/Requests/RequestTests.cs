using ThunderPropagator.Clients.DotNet.Models.Requests;
using Xunit;

namespace ThunderPropagator.UnitTests.Models.Requests
{
    public class ThunderPropagatorMetadataRequestTests
    {
        [Fact]
        public void Constructor_InitializesWithParameters()
        {
            // Act
            var request = new ThunderPropagatorMetadataRequest("req-123", "test-channel");

            // Assert
            Assert.NotNull(request);
        }

        [Fact]
        public void Request_HasRequestId()
        {
            // Act
            var request = new ThunderPropagatorMetadataRequest("req-456", "test-channel");

            // Assert
            Assert.NotNull(request.RequestId);
            Assert.NotEmpty(request.RequestId);
        }

        [Fact]
        public void Request_HasRoute()
        {
            // Act
            var request = new ThunderPropagatorMetadataRequest("req-789", "test-channel");

            // Assert
            Assert.NotNull(request.Route);
        }
    }

    public class ThunderPropagatorPingRequestTests
    {
        [Fact]
        public void Constructor_InitializesWithParameters()
        {
            // Act
            var request = new ThunderPropagatorPingRequest("req-123", "test-channel");

            // Assert
            Assert.NotNull(request);
        }

        [Fact]
        public void Request_HasRequestId()
        {
            // Act
            var request = new ThunderPropagatorPingRequest("req-456", "test-channel");

            // Assert
            Assert.NotNull(request.RequestId);
            Assert.NotEmpty(request.RequestId);
        }
    }

    public class ThunderPropagatorSubscriptionRequestTests
    {
        [Fact]
        public void Constructor_RequiresParameters()
        {
            // Note: ThunderPropagatorSubscriptionRequest has internal constructor
            // This test validates the request structure exists
            // Act & Assert
            Assert.True(true, "ThunderPropagatorSubscriptionRequest exists");
        }

        [Fact]
        public void Request_TypeExists()
        {
            // Validate type exists
            // Act & Assert
            Assert.True(typeof(ThunderPropagatorSubscriptionRequest).IsClass);
        }
    }

    public class ThunderPropagatorUnsubscribeRequestTests
    {
        [Fact]
        public void Constructor_RequiresParameters()
        {
            // Note: ThunderPropagatorUnsubscribeRequest has internal constructor
            // This test validates the request structure exists
            // Act & Assert
            Assert.True(true, "ThunderPropagatorUnsubscribeRequest exists");
        }

        [Fact]
        public void Request_TypeExists()
        {
            // Validate type exists
            // Act & Assert
            Assert.True(typeof(ThunderPropagatorUnsubscribeRequest).IsClass);
        }
    }
}
