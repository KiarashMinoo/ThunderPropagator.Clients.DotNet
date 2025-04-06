using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace RapidStreamer.Clients.DotNet.Models.Connections
{
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerConnectionResponse
    {
        [JsonProperty, JsonInclude] public string ConnectionId { get; private set; } = null!;

        [JsonProperty, JsonInclude] public bool IsAvailable { get; private set; }

        [JsonProperty, JsonInclude] public RapidStreamerPushMessageConfiguration PushMessageConfiguration { get; private set; } = null!;
    }
}