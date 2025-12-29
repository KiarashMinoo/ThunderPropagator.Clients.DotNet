using ThunderPropagator.Clients.DotNet.Models.Requests;
using Xunit;

namespace ThunderPropagator.UnitTests.Models.Requests
{
    public class RequestIdHelperTests
    {
        [Fact]
        public void Generate_ReturnsNonEmptyString()
        {
            // Act
            var requestId = RequestIdHelper.Generate();

            // Assert
            Assert.NotNull(requestId);
            Assert.NotEmpty(requestId);
        }

        [Fact]
        public void Generate_ReturnsUniqueIds()
        {
            // Act
            var id1 = RequestIdHelper.Generate();
            var id2 = RequestIdHelper.Generate();

            // Assert
            Assert.NotEqual(id1, id2);
        }

        [Fact]
        public void Generate_MultipleCallsReturnDifferentValues()
        {
            // Arrange
            var ids = new HashSet<string>();

            // Act
            for (int i = 0; i < 100; i++)
            {
                ids.Add(RequestIdHelper.Generate());
            }

            // Assert
            Assert.Equal(100, ids.Count);
        }
    }
}
