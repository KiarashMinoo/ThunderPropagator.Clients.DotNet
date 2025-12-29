using ThunderPropagator.BuildingBlocks.Application.Objects;

namespace ThunderPropagator.Clients.DotNet.Infrastructure.Requests
{
    public abstract class ThunderPropagatorRequestBase : DisposableObject
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private ManualResetEvent? _requestManualResetEvent;

        public string RequestId { get; }
        public ThunderPropagatorRequestRoute Route { get; }
        public string? Token { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        internal bool Awaitable { get; private set; }

        protected ThunderPropagatorRequestBase(string requestId, ThunderPropagatorRequestRoute route)
        {
            RequestId = requestId;
            Route = route;
        }

        protected ThunderPropagatorRequestBase(string requestId, string channelName, string requestType)
            : this(requestId, new ThunderPropagatorRequestRoute(channelName, requestType))
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