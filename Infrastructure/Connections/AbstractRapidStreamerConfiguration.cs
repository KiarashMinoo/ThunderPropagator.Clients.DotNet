using RapidStreamer.BuildingBlocks.Application;

namespace RapidStreamer.Clients.DotNet.Infrastructure.Connections
{
    public abstract class AbstractRapidStreamerConfiguration : ServiceConfiguration
    {
        public string Uri
        {
            get => Get<string>()!;
            set => Set(value);
        }
    }
}