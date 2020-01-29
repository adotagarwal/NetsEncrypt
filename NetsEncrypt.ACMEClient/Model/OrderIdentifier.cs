
using Newtonsoft.Json;

namespace NetsEncrypt.ACMEClient.Model
{
    public class OrderIdentifier
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

    }
}
