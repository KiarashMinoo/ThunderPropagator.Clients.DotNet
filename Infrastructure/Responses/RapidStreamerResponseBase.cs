using Newtonsoft.Json;
using RapidStreamer.Clients.DotNet.Infrastructure.Requests;
using System.Text.Json.Serialization;

namespace RapidStreamer.Clients.DotNet.Infrastructure.Responses
{
    public class RapidStreamerResponseBase
    {
        [JsonProperty, JsonInclude] public string ConnectionId { get; private set; } = null!;

        [JsonProperty, JsonInclude] public string RequestId { get; private set; } = null!;

        [JsonProperty, JsonInclude] public int ResponseCode { get; private set; }

        [JsonProperty, JsonInclude] public string ResponseContent { get; private set; } = null!;

        [JsonProperty, JsonInclude] public RapidStreamerRequestRoute Route { get; private set; } = null!;
    }
}