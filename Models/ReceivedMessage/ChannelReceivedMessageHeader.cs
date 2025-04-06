namespace RapidStreamer.Clients.DotNet.Models.ReceivedMessage
{
    public
#if !DEBUG
        sealed
#endif
        class ChannelReceivedMessageHeader
    {
        private readonly int _hashCode;

        public string ChannelName { get; }
        public string RequestId { get; }
        public bool FromSnapshot { get; }
        public string RecordStatus { get; }

        public ChannelReceivedMessageHeader(string channelName, string requestId, bool fromSnapshot, string recordStatus)
        {
            ChannelName = channelName;
            RequestId = requestId;
            FromSnapshot = fromSnapshot;
            RecordStatus = recordStatus;

            _hashCode = HashCode.Combine(ChannelName, RequestId, FromSnapshot, RecordStatus);
        }

        public override int GetHashCode() => _hashCode;
    }
}