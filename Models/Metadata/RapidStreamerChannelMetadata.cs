using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace RapidStreamer.Clients.DotNet.Models.Metadata
{
    public
#if !DEBUG
        sealed
#endif
        class RapidStreamerChannelMetadata
    {
        [JsonProperty("channelName"), JsonPropertyName("channelName"), JsonInclude]
        public string ChannelName { get; private set; } = null!;


        [JsonProperty("description"), JsonPropertyName("description"), JsonInclude]
        public string Description { get; private set; } = null!;


        [JsonProperty("channelProgramsDescriptors"), JsonPropertyName("channelProgramsDescriptors"), JsonInclude]
        public IReadOnlyDictionary<int, RapidStreamerChannelProgramsDescriptor> ChannelProgramsDescriptors { get; private set; } = null!;


        [JsonProperty("requestsDescriptors"), JsonPropertyName("requestsDescriptors"), JsonInclude]
        public List<RapidStreamerChannelRequestsDescriptor> RequestsDescriptors { get; private set; } = null!;


        [JsonProperty("snapshot"), JsonPropertyName("snapshot"), JsonInclude]
        public RapidStreamerChannelSnapshot ChannelSnapshot { get; private set; } = null!;


        [JsonProperty("authentication"), JsonPropertyName("authentication"), JsonInclude]
        public RapidStreamerChannelAuthentication Authentication { get; private set; } = null!;


        [JsonProperty("authorization"), JsonPropertyName("authorization"), JsonInclude]
        public RapidStreamerChannelAuthorization Authorization { get; private set; } = null!;


        [JsonProperty("messageEncryption"), JsonPropertyName("messageEncryption"), JsonInclude]
        public RapidStreamerChannelMessageEncryption MessageEncryption { get; private set; } = null!;

        public RapidStreamerChannelProgramsDescriptor? this[int index] => ChannelProgramsDescriptors.GetValueOrDefault(index);
    }
}