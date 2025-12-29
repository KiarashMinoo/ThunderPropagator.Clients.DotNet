using Newtonsoft.Json;
using ThunderPropagator.Clients.DotNet.Models.Enums;
using System.Text.Json.Serialization;

namespace ThunderPropagator.Clients.DotNet.Models.Metadata
{
    public
#if !DEBUG
        sealed
#endif
        class ThunderPropagatorChannelAuthentication
    {
        [JsonProperty("isEnabled"), JsonPropertyName("isEnabled"), JsonInclude]
        public bool IsEnabled { get; private set; }


        [JsonProperty("authenticationType"), JsonPropertyName("authenticationType"), JsonInclude]
        public ThunderPropagatorChannelAuthenticationType AuthenticationType { get; private set; }
    }
}