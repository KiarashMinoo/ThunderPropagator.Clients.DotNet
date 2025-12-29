namespace ThunderPropagator.Clients.DotNet.Models.ReceivedMessage
{
    public
#if !DEBUG
        sealed
#endif
        class ChannelReceivedMessage
    {
        public ChannelReceivedMessageHeader Header { get; }
        public string[] Keys { get; }
        public IReadOnlyDictionary<int, string> Values { get; }

        public ChannelReceivedMessage(ChannelReceivedMessageHeader header, string[] keys, IReadOnlyDictionary<int, string> values)
        {
            Header = header;
            Keys = keys;
            Values = values;
        }
    }
}