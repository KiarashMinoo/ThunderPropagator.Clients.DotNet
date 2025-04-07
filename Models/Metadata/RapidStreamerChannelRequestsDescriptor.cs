using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace RapidStreamer.Clients.DotNet.Models.Metadata
{
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerChannelRequestsDescriptor
    {
        [JsonProperty("route"), JsonPropertyName("route"), JsonInclude]
        public string Route { get; private set; } = null!;

        [JsonProperty("requestSchema"), JsonPropertyName("requestSchema"), JsonInclude]
        public string RequestSchema { get; private set; } = null!;

        [JsonProperty("responseSchema"), JsonPropertyName("responseSchema"), JsonInclude]
        public string ResponseSchema { get; private set; } = null!;

        [JsonProperty("exceptions"), JsonPropertyName("exceptions"), JsonInclude]
        public List<string> Exceptions { get; private set; } = null!;
    }
}