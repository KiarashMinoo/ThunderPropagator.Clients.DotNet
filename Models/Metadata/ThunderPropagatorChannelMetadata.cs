using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ThunderPropagator.Clients.DotNet.Models.Metadata
{
    public
#if !DEBUG
        sealed
#endif
        class ThunderPropagatorChannelMetadata
    {
        [JsonProperty("channelName"), JsonPropertyName("channelName"), JsonInclude]
        public string ChannelName { get; private set; } = null!;


        [JsonProperty("description"), JsonPropertyName("description"), JsonInclude]
        public string Description { get; private set; } = null!;


        [JsonProperty("channelProgramsDescriptors"), JsonPropertyName("channelProgramsDescriptors"), JsonInclude]
        public IReadOnlyDictionary<int, ThunderPropagatorChannelProgramsDescriptor> ChannelProgramsDescriptors { get; private set; } = null!;


        [JsonProperty("requestsDescriptors"), JsonPropertyName("requestsDescriptors"), JsonInclude]
        public List<ThunderPropagatorChannelRequestsDescriptor> RequestsDescriptors { get; private set; } = null!;


        [JsonProperty("snapshot"), JsonPropertyName("snapshot"), JsonInclude]
        public ThunderPropagatorChannelSnapshot ChannelSnapshot { get; private set; } = null!;


        [JsonProperty("authentication"), JsonPropertyName("authentication"), JsonInclude]
        public ThunderPropagatorChannelAuthentication Authentication { get; private set; } = null!;


        [JsonProperty("authorization"), JsonPropertyName("authorization"), JsonInclude]
        public ThunderPropagatorChannelAuthorization Authorization { get; private set; } = null!;


        [JsonProperty("messageEncryption"), JsonPropertyName("messageEncryption"), JsonInclude]
        public ThunderPropagatorChannelMessageEncryption MessageEncryption { get; private set; } = null!;

        public ThunderPropagatorChannelProgramsDescriptor? this[int index] => ChannelProgramsDescriptors.GetValueOrDefault(index);
    }
}