using System.Security.Cryptography;

namespace LetsEncryptClient.CacheStore
{
    public class CachedCertificateResult
    {
        public RSA PrivateKey;
        public string Certificate;
    }
}
