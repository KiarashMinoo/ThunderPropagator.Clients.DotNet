using ThunderPropagator.Clients.DotNet.Infrastructure.Connections;
using ThunderPropagator.Clients.DotNet.Infrastructure.Loggers;
using System.Threading.Channels;

namespace ThunderPropagator.Clients.DotNet.Connections.InfiniteDataStream
{
    internal
#if !DEBUG
        sealed
#endif
        class ThunderPropagatorInfiniteDataStreamConnection : AbstractThunderPropagatorConnection<ThunderPropagatorInfiniteDataStreamConnectionConfiguration>
    {
        private readonly HttpClient _httpClient;

        private readonly Channel<string> _receivedMessagesChannel;

        public ThunderPropagatorInfiniteDataStreamConnection(ThunderPropagatorInfiniteDataStreamConnectionConfiguration connectionConfiguration, ILoggerProvider loggerProvider)
            : base(connectionConfiguration, loggerProvider)
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite),
                BaseAddress = new Uri(connectionConfiguration.Uri)
            };

            _receivedMessagesChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true
            });
        }

        protected override async Task InternalConnectAsync(CancellationToken cancellationToken = default)
            => await _receivedMessagesChannel.Writer.WriteAsync("PROBE", cancellationToken);

        protected override Task InternalDisconnectAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        protected override ValueTask<string> ReceiveAsync(CancellationToken cancellationToken = default) => _receivedMessagesChannel.Reader.ReadAsync(cancellationToken);
        protected override Task InternalSendAsync(string message, CancellationToken cancellationToken) => Task.CompletedTask;

        internal Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage,
            HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead,
            CancellationToken cancellationToken = default)
            => _httpClient.SendAsync(requestMessage, httpCompletionOption, cancellationToken);

        internal ValueTask AddMessageAsync(string message, CancellationToken cancellationToken = default) => _receivedMessagesChannel.Writer.WriteAsync(message, cancellationToken);

        protected override void DisposeManagedResources()
        {
            _httpClient.Dispose();
        }
    }
}