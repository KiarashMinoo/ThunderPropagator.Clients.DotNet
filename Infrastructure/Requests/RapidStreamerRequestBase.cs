using RapidStreamer.BuildingBlocks.Application.Objects;

namespace RapidStreamer.Clients.DotNet.Infrastructure.Requests
{
    public abstract class RapidStreamerRequestBase : DisposableObject
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private ManualResetEvent? _requestManualResetEvent;

        public string RequestId { get; }
        public RapidStreamerRequestRoute Route { get; }
        public string? Token { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        internal bool Awaitable { get; private set; }

        protected RapidStreamerRequestBase(string requestId, RapidStreamerRequestRoute route)
        {
            RequestId = requestId;
            Route = route;
        }

        protected RapidStreamerRequestBase(string requestId, string channelName, string requestType)
            : this(requestId, new RapidStreamerRequestRoute(channelName, requestType))
        {
        }

        internal ManualResetEvent SetAwaitable()
        {
            Awaitable = true;
            return _requestManualResetEvent = new ManualResetEvent(false);
        }

        internal void SetRequestTimeout(TimeSpan timeSpan, Action cancellationCallback)
        {
            _cancellationTokenSource = new CancellationTokenSource(timeSpan);
            _cancellationTokenSource.Token.Register(cancellationCallback);
        }

        protected override void DisposeManagedResources()
        {
            _requestManualResetEvent?.Set();
            _requestManualResetEvent?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}