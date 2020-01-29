using Newtonsoft.Json;

namespace NetsEncrypt.ACMEClient.Model
{
    
    public class FinalizeRequest
    {
        [JsonProperty("csr")]
        public string CSR { get; set; }
    }

}
