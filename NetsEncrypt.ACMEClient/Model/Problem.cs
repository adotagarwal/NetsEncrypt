using Newtonsoft.Json;

namespace NetsEncrypt.ACMEClient.Model
{
    public class Problem
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }

        public string RawJson { get; set; }
    }

}
