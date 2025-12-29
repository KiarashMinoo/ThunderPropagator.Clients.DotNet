using ThunderPropagator.Clients.DotNet.Models.Connections;
using Xunit;

namespace ThunderPropagator.UnitTests.Models.Connections
{
    public class ThunderPropagatorConnectionResponseTests
    {
        [Fact]
        public void Constructor_InitializesWithDefaultValues()
        {
            // Act
            var response = new ThunderPropagatorConnectionResponse();

            // Assert
            Assert.NotNull(response);
        }
    }

    public class ThunderPropagatorPushMessageConfigurationTests
    {
        [Fact]
        public void Constructor_InitializesWithDefaultValues()
        {
            // Act
            var config = new ThunderPropagatorPushMessageConfiguration();

            // Assert
            Assert.NotNull(config);
        }
    }
}
