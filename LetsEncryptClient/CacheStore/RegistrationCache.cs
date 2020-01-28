using LetsEncryptClient.ACME;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace LetsEncryptClient.CacheStore
{
    internal class RegistrationCache
    {
        private static readonly object _locker = new object();
        private static volatile RegistrationCache _cache = null;

        public static string GetDefaultCacheFilename(string apiUrl)
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);
            var hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(apiUrl));
            var file = ACMEEncryptor.Base64UrlEncoded(hash) + ".lets-encrypt.cache.json";
            return Path.Combine(home, file);
        }

        public static void SetInstance(RegistrationCache cache)
        {
            if (_cache == null)
                lock (_locker)
                    if (_cache ==null)
                        _cache = cache;
        }

        public static RegistrationCache Instance => _cache;

        public static void SaveInstance(string fileName)
        {
            lock (_locker)
            {
                File.WriteAllText(fileName, JsonConvert.SerializeObject(_cache, Newtonsoft.Json.Formatting.Indented));
            }
        }

        public static RegistrationCache LoadCacheFromFile(string fileName)
        {
            return JsonConvert.DeserializeObject<RegistrationCache>(File.ReadAllText(fileName));
        }

        public static void ResetCachedCertificate(IEnumerable<string> hostsToRemove)
        {
            foreach (var host in hostsToRemove)
            {
                Instance?.CachedCerts?.Remove(host);
            }
        }

        public static bool TryGetCachedCertificate(List<string> hosts, out CachedCertificateResult value)
        {
            value = null;
            if (Instance.CachedCerts.TryGetValue(hosts[0], out var cache) == false)
            {
                return false;
            }

            var cert = new X509Certificate2(cache.Cert);

            // if it is about to expire, we need to refresh
            if ((cert.NotAfter - DateTime.UtcNow).TotalDays < 14)
                return false;

            var rsa = new RSACryptoServiceProvider(4096);
            rsa.ImportCspBlob(cache.Private);

            value = new CachedCertificateResult
            {
                Certificate = cache.Cert,
                PrivateKey = rsa
            };
            return true;
        }

        public readonly Dictionary<string, CertificateCache> CachedCerts = new Dictionary<string, CertificateCache>(StringComparer.OrdinalIgnoreCase);
        public byte[] AccountKey;
        public string Id;
        public ACMEPrivateKey Key;
        public Uri Location;
    }
}
