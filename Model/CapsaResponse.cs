using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Capsa_Connector.Model
{
    public class CapsaSocketResponse
    {
        public string command { get; set; }
        public string socketId { get; set; }
        public DateTime serverTime { get; set; }
        public DataSocketContainer data { get; set; }
    }
    public class DataSocketContainer
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<int> data { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string fileAccessToken { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string fileURL { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string appToken { get; set; }
    }


    public class CapsaTokenResponseWrap
    {
        public CapsaTokenResponse data;
    }
    public class CapsaTokenResponse
    {
        public string appToken { get; set; }
        public string pairingAccessToken { get; set; }
    }
}
