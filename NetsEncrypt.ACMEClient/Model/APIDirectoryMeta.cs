
using Newtonsoft.Json;

namespace NetsEncrypt.ACMEClient.Model
{
    public class APIDirectoryMeta
    {
        [JsonProperty("termsOfService")]
        public string TermsOfService { get; set; }
    }
}
