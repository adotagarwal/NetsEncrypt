using System;
using Newtonsoft.Json;

namespace NetsEncrypt.ACMEClient.Model
{
    public class AuthorizationChallenge
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

    }
}
