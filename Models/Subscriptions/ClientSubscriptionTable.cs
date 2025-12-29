using ThunderPropagator.BuildingBlocks.Application.Collections;

namespace ThunderPropagator.Clients.DotNet.Models.Subscriptions
{
    internal class ClientSubscriptionTable
    {
        public required string TableName { get; init; }
        public required Func<IReadOnlyDictionary<int, string>, string> TableKey { get; init; }
        public BindingDictionary<string, BindingDictionary<int, ThunderPropagatorSubscriptionItemUpdate>> Table { get; } = [];

        public override int GetHashCode() => TableName.GetHashCode();
    }
}