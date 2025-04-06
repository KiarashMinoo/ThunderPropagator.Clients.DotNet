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
        [JsonProperty, JsonInclude] public int Index { get; private set; }

        [JsonProperty, JsonInclude] public string Name { get; private set; } = null!;

        [JsonProperty, JsonInclude] public RapidStreamerChannelFieldType Type { get; private set; }

        [JsonProperty, JsonInclude] public string? Description { get; private set; }

        [JsonProperty, JsonInclude] public string? Table { get; private set; }

        [JsonProperty, JsonInclude] public bool IsSubscribingKey { get; private set; }

        [JsonProperty, JsonInclude] public bool IsSubscribingField { get; private set; }
    }
}