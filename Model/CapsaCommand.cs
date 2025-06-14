using Newtonsoft.Json;

namespace Capsa_Connector.Model
{
    public class CapsaSocketCommand
    {
        public string? command { get; set; }
        public DataCommandContainer? data { get; set; }
    }
    public class DataCommandContainer
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? fileAccessToken { get; set; }
    }
}
