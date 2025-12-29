using ThunderPropagator.BuildingBlocks.Application;

namespace ThunderPropagator.Clients.DotNet.Infrastructure.Connections
{
    public abstract class AbstractThunderPropagatorConfiguration : ServiceConfiguration
    {
        public string Uri
        {
            get => Get<string>()!;
            set => Set(value);
        }
    }
}