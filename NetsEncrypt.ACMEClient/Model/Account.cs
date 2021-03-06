﻿using System;
using NetsEncrypt.ACMEClient.JsonWebSignature;
using Newtonsoft.Json;

namespace NetsEncrypt.ACMEClient.Model
{
    public class Account : IHasLocation
    {
        [JsonProperty("termsOfServiceAgreed")]
        public bool TermsOfServiceAgreed { get; set; }

        [JsonProperty("contact")]
        public string[] Contacts { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("key")]
        public JsonWebKey Key { get; set; }

        [JsonProperty("initialIp")]
        public string InitialIp { get; set; }

        [JsonProperty("orders")]
        public Uri Orders { get; set; }

        public Uri Location { get; set; }
    }
}
