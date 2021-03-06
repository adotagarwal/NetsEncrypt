﻿using System;
using NetsEncrypt.ACMEClient.JsonWebSignature;
using Newtonsoft.Json;

namespace NetsEncrypt.ACMEClient.ACME
{
    /// <summary>
    /// JwsHeader
    /// </summary>
    internal class ACMERequestHeader
    {
        public ACMERequestHeader()
        {
        }

        public ACMERequestHeader(string algorithm, JsonWebKey key)
        {
            Algorithm = algorithm;
            Key = key;
        }

        [JsonProperty("alg")]
        public string Algorithm { get; set; }

        [JsonProperty("jwk")]
        public JsonWebKey Key { get; set; }


        [JsonProperty("kid")]
        public string KeyId { get; set; }


        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }
}
