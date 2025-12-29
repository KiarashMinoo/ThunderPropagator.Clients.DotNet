namespace ThunderPropagator.Clients.DotNet.Models
{
    public
#if !DEBUG
        sealed
#endif
        class CipheringMetadata
    {
        public int KeySize { get; init; } = 512;
        public required string Key { get; init; }
    }
}