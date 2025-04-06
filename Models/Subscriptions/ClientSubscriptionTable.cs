using RapidStreamer.BuildingBlocks.Application.Collections;

namespace RapidStreamer.Clients.DotNet.Models.Subscriptions
{
    internal class ClientSubscriptionTable
    {
        public required string TableName { get; init; }
        public required Func<IReadOnlyDictionary<int, string>, string> TableKey { get; init; }
        public BindingDictionary<string, BindingDictionary<int, RapidStreamerSubscriptionItemUpdate>> Table { get; } = [];

        public override int GetHashCode() => TableName.GetHashCode();
    }
}