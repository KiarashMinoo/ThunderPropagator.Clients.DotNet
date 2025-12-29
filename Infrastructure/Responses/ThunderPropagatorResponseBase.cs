using Newtonsoft.Json;
using ThunderPropagator.Clients.DotNet.Infrastructure.Requests;
using System.Text.Json.Serialization;

namespace ThunderPropagator.Clients.DotNet.Infrastructure.Responses
{
    public class ThunderPropagatorResponseBase
    {
        [JsonProperty, JsonInclude] public string ConnectionId { get; private set; } = null!;

        [JsonProperty, JsonInclude] public string RequestId { get; private set; } = null!;

        [JsonProperty, JsonInclude] public int ResponseCode { get; private set; }

        [JsonProperty, JsonInclude] public string ResponseContent { get; private set; } = null!;

        [JsonProperty, JsonInclude] public ThunderPropagatorRequestRoute Route { get; private set; } = null!;
    }
}