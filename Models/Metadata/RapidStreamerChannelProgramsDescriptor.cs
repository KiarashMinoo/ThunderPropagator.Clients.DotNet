using Newtonsoft.Json;
using RapidStreamer.Clients.DotNet.Models.Enums;
using System.Text.Json.Serialization;

namespace RapidStreamer.Clients.DotNet.Models.Metadata
{
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerChannelProgramsDescriptor
    {
        [JsonProperty("index"), JsonPropertyName("index"), JsonInclude]
        public int Index { get; private set; }


        [JsonProperty("name"), JsonPropertyName("name"), JsonInclude]
        public string Name { get; private set; } = null!;


        [JsonProperty("type"), JsonPropertyName("type"), JsonInclude]
        public RapidStreamerChannelFieldType Type { get; private set; }


        [JsonProperty("description"), JsonPropertyName("description"), JsonInclude]
        public string? Description { get; private set; }


        [JsonProperty("table"), JsonPropertyName("table"), JsonInclude]
        public string? Table { get; private set; }


        [JsonProperty("isSubscribingKey"), JsonPropertyName("isSubscribingKey"), JsonInclude]
        public bool IsSubscribingKey { get; private set; }


        [JsonProperty("isSubscribingField"), JsonPropertyName("isSubscribingField"), JsonInclude]
        public bool IsSubscribingField { get; private set; }
    }
}