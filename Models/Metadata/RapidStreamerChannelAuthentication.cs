using Newtonsoft.Json;
using RapidStreamer.Clients.DotNet.Models.Enums;
using System.Text.Json.Serialization;

namespace RapidStreamer.Clients.DotNet.Models.Metadata
{
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerChannelAuthentication
    {
        [JsonProperty("isEnabled"), JsonPropertyName("isEnabled"), JsonInclude]
        public bool IsEnabled { get; private set; }


        [JsonProperty("authenticationType"), JsonPropertyName("authenticationType"), JsonInclude]
        public RapidStreamerChannelAuthenticationType AuthenticationType { get; private set; }
    }
}