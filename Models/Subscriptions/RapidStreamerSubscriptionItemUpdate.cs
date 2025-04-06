using RapidStreamer.Clients.DotNet.Models.Enums;

namespace RapidStreamer.Clients.DotNet.Models.Subscriptions
{
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerSubscriptionItemUpdate
    {
        private string _fieldValue = null!;

        public required string[] Keys { get; init; }
        public required int FieldIndex { get; init; }
        public required string FieldName { get; init; }
        public required RapidStreamerChannelFieldType FieldType { get; init; }

        public string FieldValue
        {
            get => _fieldValue;
            internal set
            {
                Changed = !string.Equals(PreviousValue, value);

                if (Changed)
                {
                    PreviousValue = _fieldValue;
                    _fieldValue = value.Trim();
                }
            }
        }

        public bool IsSnapshot { get; internal set; }
        public bool Changed { get; private set; }
        public string? PreviousValue { get; private set; }
    }
}