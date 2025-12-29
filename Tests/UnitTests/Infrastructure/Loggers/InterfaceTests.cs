using NSubstitute;
using ThunderPropagator.Clients.DotNet.Infrastructure.Loggers;
using Xunit;

namespace ThunderPropagator.UnitTests.Infrastructure.Loggers
{
    public class ILoggerTests
    {
        [Fact]
        public void ILogger_CanLog()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();

            // Act
            logger.Log(LogLevel.Information, null, "Test message");

            // Assert
            logger.Received(1).Log(LogLevel.Information, Arg.Any<Exception?>(), Arg.Any<string>());
        }

        [Fact]
        public void ILogger_CanLogWithException()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var exception = new System.Exception("Test exception");

            // Act
            logger.Log(LogLevel.Warning, exception, "Test warning");

            // Assert
            logger.Received(1).Log(LogLevel.Warning, exception, "Test warning");
        }

        [Fact]
        public void ILogger_CanLogWithParameters()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();

            // Act
            logger.Log(LogLevel.Error, null, "Error: {0}", "test error");

            // Assert
            logger.Received(1).Log(LogLevel.Error, Arg.Any<Exception?>(), Arg.Any<string>(), Arg.Any<object[]>());
        }
    }

    public class ILoggerProviderTests
    {
        [Fact]
        public void ILoggerProvider_CanCreateLogger()
        {
            // Arrange
            var loggerProvider = Substitute.For<ILoggerProvider>();
            var mockLogger = Substitute.For<ILogger>();
            loggerProvider.CreateLogger(Arg.Any<string>()).Returns(mockLogger);

            // Act
            var logger = loggerProvider.CreateLogger("TestLogger");

            // Assert
            Assert.NotNull(logger);
            loggerProvider.Received(1).CreateLogger("TestLogger");
        }

        [Fact]
        public void ILoggerProvider_CreateLoggerWithDifferentNames()
        {
            // Arrange
            var loggerProvider = Substitute.For<ILoggerProvider>();
            var mockLogger1 = Substitute.For<ILogger>();
            var mockLogger2 = Substitute.For<ILogger>();
            loggerProvider.CreateLogger("Logger1").Returns(mockLogger1);
            loggerProvider.CreateLogger("Logger2").Returns(mockLogger2);

            // Act
            var logger1 = loggerProvider.CreateLogger("Logger1");
            var logger2 = loggerProvider.CreateLogger("Logger2");

            // Assert
            Assert.NotNull(logger1);
            Assert.NotNull(logger2);
            loggerProvider.Received(1).CreateLogger("Logger1");
            loggerProvider.Received(1).CreateLogger("Logger2");
        }
    }
}
