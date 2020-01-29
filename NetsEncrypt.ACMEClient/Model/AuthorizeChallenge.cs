
using Newtonsoft.Json;

namespace NetsEncrypt.ACMEClient.Model
{
    public class AuthorizeChallenge
    {
        [JsonProperty("keyAuthorization")]
        public string KeyAuthorization { get; set; }

    }
}
