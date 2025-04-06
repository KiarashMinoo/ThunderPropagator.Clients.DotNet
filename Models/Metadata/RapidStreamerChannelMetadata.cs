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
        [JsonProperty, JsonInclude] public string ChannelName { get; private set; } = null!;

        [JsonProperty, JsonInclude] public IReadOnlyDictionary<int, RapidStreamerChannelProgramsDescriptor> ChannelProgramsDescriptors { get; private set; } = null!;

        [JsonProperty, JsonInclude] public RapidStreamerChannelSnapshot ChannelSnapshot { get; private set; } = null!;

        [JsonProperty, JsonInclude] public RapidStreamerChannelAuthentication Authentication { get; private set; } = null!;

        [JsonProperty, JsonInclude] public RapidStreamerChannelAuthorization Authorization { get; private set; } = null!;

        [JsonProperty, JsonInclude] public RapidStreamerChannelMessageEncryption MessageEncryption { get; private set; } = null!;

        public RapidStreamerChannelProgramsDescriptor? this[int index] => ChannelProgramsDescriptors.GetValueOrDefault(index);
    }
}