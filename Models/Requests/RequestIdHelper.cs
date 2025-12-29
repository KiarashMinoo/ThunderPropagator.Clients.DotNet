namespace ThunderPropagator.Clients.DotNet.Models.Requests
{
    public static class RequestIdHelper
    {
        public static string Generate() => Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 16);
    }
}