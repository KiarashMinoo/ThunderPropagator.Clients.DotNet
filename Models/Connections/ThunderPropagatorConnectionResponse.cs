using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ThunderPropagator.Clients.DotNet.Models.Connections
{
    public
#if !DEBUG
        sealed
#endif
        class ThunderPropagatorConnectionResponse
    {
        [JsonProperty, JsonInclude] public string ConnectionId { get; private set; } = null!;

        [JsonProperty, JsonInclude] public bool IsAvailable { get; private set; }

        [JsonProperty, JsonInclude] public ThunderPropagatorPushMessageConfiguration PushMessageConfiguration { get; private set; } = null!;
    }
}