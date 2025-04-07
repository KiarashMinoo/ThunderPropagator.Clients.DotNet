using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace RapidStreamer.Clients.DotNet.Models.Metadata
{
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerChannelSnapshot
    {
        [JsonProperty("isEnabled"), JsonPropertyName("isEnabled"), JsonInclude]
        public bool IsEnabled { get; private set; }


        [JsonProperty("ttl"), JsonPropertyName("ttl"), JsonInclude]
        public TimeSpan? Ttl { get; private set; }


        [JsonProperty("isCompressed"), JsonPropertyName("isCompressed"), JsonInclude]
        public bool IsCompressed { get; private set; }


        [JsonProperty("enableHibernation"), JsonPropertyName("enableHibernation"), JsonInclude]
        public bool EnableHibernation { get; private set; }


        [JsonProperty("hibernationDueTime"), JsonPropertyName("hibernationDueTime"), JsonInclude]
        public int HibernationDueTime { get; private set; }


        [JsonProperty("isTimeSeries"), JsonPropertyName("isTimeSeries"), JsonInclude]
        public bool IsTimeSeries { get; private set; }
    }
}