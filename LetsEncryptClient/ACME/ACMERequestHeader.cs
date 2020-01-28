
using Newtonsoft.Json;
using System;

namespace LetsEncryptClient.ACME
{
    /// <summary>
    /// JwsHeader
    /// </summary>
    internal class ACMERequestHeader
    {
        public ACMERequestHeader()
        {
        }

        public ACMERequestHeader(string algorithm, ACMEPrivateKey key)
        {
            Algorithm = algorithm;
            Key = key;
        }

        [JsonProperty("alg")]
        public string Algorithm { get; set; }

        [JsonProperty("jwk")]
        public ACMEPrivateKey Key { get; set; }


        [JsonProperty("kid")]
        public string KeyId { get; set; }


        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }
}
