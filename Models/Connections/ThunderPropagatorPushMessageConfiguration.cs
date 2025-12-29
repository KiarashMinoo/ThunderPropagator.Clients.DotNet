using ThunderPropagator.BuildingBlocks.Application;

namespace ThunderPropagator.Clients.DotNet.Models.Connections
{
    public
#if !DEBUG
        sealed
#endif
        class ThunderPropagatorPushMessageConfiguration : ServiceConfiguration
    {
        public string Path => Get<string>()!;
        public int BufferSize => Get<int>();
        public string SubProtocol => Get<string>()!;
        public bool DangerousEnableCompression => Get<bool>();
        public bool DisableServerContextTakeover => Get<bool>();
        public int ServerMaxWindowBits => Get<int>();
        public int MaxRequestSize => Get<int>();
        public int MaxPushSize => Get<int>();
        public TimeSpan KeepAliveInterval => Get<TimeSpan>();
        public int ReceiveBufferSize => Get<int>();
        public string[] AllowedOrigins => Get<string[]>()!;
    }
}