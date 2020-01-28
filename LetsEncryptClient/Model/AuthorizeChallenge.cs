
using Newtonsoft.Json;

namespace LetsEncryptClient.Model
{
    public class AuthorizeChallenge
    {
        [JsonProperty("keyAuthorization")]
        public string KeyAuthorization { get; set; }

    }
}
