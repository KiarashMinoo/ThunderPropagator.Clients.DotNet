using ThunderPropagator.Clients.DotNet.Models.Enums;
using Xunit;

namespace ThunderPropagator.UnitTests.Models.Enums
{
    public class ThunderPropagatorSubscriptionModeTests
    {
        [Fact]
        public void SubscriptionMode_ValuesAreUnique()
        {
            // Arrange
            var allValues = Enum.GetValues<ThunderPropagatorSubscriptionMode>();

            // Act & Assert
            Assert.Equal(allValues.Length, allValues.Distinct().Count());
        }

        [Fact]
        public void SubscriptionMode_CanBeCompared()
        {
            // Arrange
            var mode1 = (ThunderPropagatorSubscriptionMode)0;
            var mode2 = (ThunderPropagatorSubscriptionMode)0;

            // Act & Assert
            Assert.Equal(mode1, mode2);
        }
    }

    public class ThunderPropagatorSubscriptionStatusTests
    {
        [Fact]
        public void SubscriptionStatus_ValuesAreUnique()
        {
            // Arrange
            var allValues = Enum.GetValues<ThunderPropagatorSubscriptionStatus>();

            // Act & Assert
            Assert.Equal(allValues.Length, allValues.Distinct().Count());
        }
    }

    public class ThunderPropagatorChannelAuthenticationTypeTests
    {
        [Fact]
        public void AuthenticationType_ValuesAreUnique()
        {
            // Arrange
            var allValues = Enum.GetValues<ThunderPropagatorChannelAuthenticationType>();

            // Act & Assert
            Assert.Equal(allValues.Length, allValues.Distinct().Count());
        }
    }

    public class ThunderPropagatorChannelFieldTypeTests
    {
        [Fact]
        public void FieldType_ValuesAreUnique()
        {
            // Arrange
            var allValues = Enum.GetValues<ThunderPropagatorChannelFieldType>();

            // Act & Assert
            Assert.Equal(allValues.Length, allValues.Distinct().Count());
        }
    }

    public class ThunderPropagatorChannelStorageTypeTests
    {
        [Fact]
        public void StorageType_ValuesAreUnique()
        {
            // Arrange
            var allValues = Enum.GetValues<ThunderPropagatorChannelStorageType>();

            // Act & Assert
            Assert.Equal(allValues.Length, allValues.Distinct().Count());
        }
    }

    public class ThunderPropagatorRecordStatusTests
    {
        [Fact]
        public void RecordStatus_ValuesAreUnique()
        {
            // Arrange
            var allValues = Enum.GetValues<ThunderPropagatorRecordStatus>();

            // Act & Assert
            Assert.Equal(allValues.Length, allValues.Distinct().Count());
        }
    }

    public class ThunderPropagatorPingPongStateTests
    {
        [Fact]
        public void PingPongState_ValuesAreUnique()
        {
            // Arrange
            var allValues = Enum.GetValues<ThunderPropagatorPingPongState>();

            // Act & Assert
            Assert.Equal(allValues.Length, allValues.Distinct().Count());
        }
    }
}
