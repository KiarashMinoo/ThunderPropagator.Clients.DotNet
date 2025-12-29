namespace ThunderPropagator.Clients.DotNet.Infrastructure.Loggers
{
    public interface ILoggerProvider : IDisposable
    {
        ILogger CreateLogger(string categoryName);
    }
}