using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace RapidStreamer.Clients.DotNet.Models.Metadata
{
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerChannelMessageEncryption
    {
        [JsonProperty, JsonInclude] public bool IsEnabled { get; internal set; }
    }
}