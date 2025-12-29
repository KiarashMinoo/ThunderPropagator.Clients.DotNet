using ThunderPropagator.Clients.DotNet.Models.ReceivedMessage;
using Xunit;

namespace ThunderPropagator.UnitTests.Models.ReceivedMessage
{
    public class ChannelReceivedMessageTests
    {
        [Fact]
        public void Constructor_InitializesAllProperties()
        {
            // Arrange
            var header = new ChannelReceivedMessageHeader("test-channel", "req-123", false, "active");
            var keys = new[] { "key1", "key2" };
            var values = new Dictionary<int, string> { { 0, "value1" }, { 1, "value2" } };

            // Act
            var message = new ChannelReceivedMessage(header, keys, values);

            // Assert
            Assert.NotNull(message.Header);
            Assert.Equal(keys, message.Keys);
            Assert.Equal(values, message.Values);
        }

        [Fact]
        public void Keys_ReturnsCorrectArray()
        {
            // Arrange
            var header = new ChannelReceivedMessageHeader("test-channel", "req-456", true, "snapshot");
            var keys = new[] { "testKey" };
            var values = new Dictionary<int, string>();

            // Act
            var message = new ChannelReceivedMessage(header, keys, values);

            // Assert
            Assert.Single(message.Keys);
            Assert.Equal("testKey", message.Keys[0]);
        }

        [Fact]
        public void Values_ReturnsCorrectDictionary()
        {
            // Arrange
            var header = new ChannelReceivedMessageHeader("channel", "req-789", false, "active");
            var keys = new string[] { };
            var values = new Dictionary<int, string> { { 5, "testValue" } };

            // Act
            var message = new ChannelReceivedMessage(header, keys, values);

            // Assert
            Assert.Single(message.Values);
            Assert.Equal("testValue", message.Values[5]);
        }
    }

    public class ChannelReceivedMessageHeaderTests
    {
        [Fact]
        public void Constructor_InitializesWithParameters()
        {
            // Act
            var header = new ChannelReceivedMessageHeader("test-channel", "req-123", false, "active");

            // Assert
            Assert.NotNull(header);
            Assert.Equal("test-channel", header.ChannelName);
            Assert.Equal("req-123", header.RequestId);
            Assert.False(header.FromSnapshot);
            Assert.Equal("active", header.RecordStatus);
        }
    }
}
