using ThunderPropagator.Clients.DotNet.Infrastructure.Loggers;
using Xunit;

namespace ThunderPropagator.UnitTests.Infrastructure.Loggers
{
    public class EventIdTests
    {
        [Fact]
        public void Constructor_WithIdAndName_InitializesCorrectly()
        {
            // Arrange
            var id = 1;
            var name = "TestEvent";

            // Act
            var eventId = new EventId(id, name);

            // Assert
            Assert.Equal(id, eventId.Id);
            Assert.Equal(name, eventId.Name);
        }

        [Fact]
        public void Constructor_WithIdOnly_InitializesCorrectly()
        {
            // Arrange
            var id = 42;

            // Act
            var eventId = new EventId(id);

            // Assert
            Assert.Equal(id, eventId.Id);
        }

        [Fact]
        public void Equals_WithSameValues_ReturnsTrue()
        {
            // Arrange
            var eventId1 = new EventId(1, "Test");
            var eventId2 = new EventId(1, "Test");

            // Act & Assert
            Assert.Equal(eventId1, eventId2);
        }

        [Fact]
        public void Equals_WithDifferentIds_ReturnsFalse()
        {
            // Arrange
            var eventId1 = new EventId(1, "Test");
            var eventId2 = new EventId(2, "Test");

            // Act & Assert
            Assert.NotEqual(eventId1, eventId2);
        }

        [Fact]
        public void GetHashCode_WithSameValues_ReturnsSameHash()
        {
            // Arrange
            var eventId1 = new EventId(1, "Test");
            var eventId2 = new EventId(1, "Test");

            // Act & Assert
            Assert.Equal(eventId1.GetHashCode(), eventId2.GetHashCode());
        }
    }

    public class LogLevelTests
    {
        [Fact]
        public void LogLevel_HasExpectedValues()
        {
            // Act & Assert
            Assert.True(Enum.IsDefined(typeof(LogLevel), LogLevel.Trace));
            Assert.True(Enum.IsDefined(typeof(LogLevel), LogLevel.Debug));
            Assert.True(Enum.IsDefined(typeof(LogLevel), LogLevel.Information));
            Assert.True(Enum.IsDefined(typeof(LogLevel), LogLevel.Warning));
            Assert.True(Enum.IsDefined(typeof(LogLevel), LogLevel.Error));
            Assert.True(Enum.IsDefined(typeof(LogLevel), LogLevel.Critical));
        }

        [Fact]
        public void LogLevel_ValuesAreOrdered()
        {
            // Act & Assert
            Assert.True(LogLevel.Trace < LogLevel.Debug);
            Assert.True(LogLevel.Debug < LogLevel.Information);
            Assert.True(LogLevel.Information < LogLevel.Warning);
            Assert.True(LogLevel.Warning < LogLevel.Error);
            Assert.True(LogLevel.Error < LogLevel.Critical);
        }
    }
}
