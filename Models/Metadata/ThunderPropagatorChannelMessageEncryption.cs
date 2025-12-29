using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ThunderPropagator.Clients.DotNet.Models.Metadata
{
    public
#if !DEBUG
        sealed
#endif
        class ThunderPropagatorChannelMessageEncryption
    {
        [JsonProperty("isEnabled"), JsonPropertyName("isEnabled"), JsonInclude]
        public bool IsEnabled { get; private set; }
    }
}