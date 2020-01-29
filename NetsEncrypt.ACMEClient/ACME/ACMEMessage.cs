using Newtonsoft.Json;

namespace NetsEncrypt.ACMEClient.ACME
{
    internal class ACMEMessage
    {
        [JsonProperty("header")]
        public ACMERequestHeader Header { get; set; }

        [JsonProperty("protected")]
        public string Protected { get; set; }

        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }
    }
}
