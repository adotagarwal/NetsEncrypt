
using Newtonsoft.Json;

namespace LetsEncryptClient.Model
{
    public class APIDirectoryMeta
    {
        [JsonProperty("termsOfService")]
        public string TermsOfService { get; set; }
    }
}
