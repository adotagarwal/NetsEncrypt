using Newtonsoft.Json;

namespace LetsEncryptClient.Model
{
    
    public class FinalizeRequest
    {
        [JsonProperty("csr")]
        public string CSR { get; set; }
    }

}
