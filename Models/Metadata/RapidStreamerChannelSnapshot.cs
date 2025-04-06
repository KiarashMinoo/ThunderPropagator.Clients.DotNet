using Newtonsoft.Json;
using RapidStreamer.Clients.DotNet.Models.Enums;
using System.Text.Json.Serialization;

namespace RapidStreamer.Clients.DotNet.Models.Metadata
{
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerChannelSnapshot
    {
        [JsonProperty, JsonInclude] public bool IsEnabled { get; private set; }

        [JsonProperty, JsonInclude] public RapidStreamerChannelStorageType Storage { get; private set; }
    }
}