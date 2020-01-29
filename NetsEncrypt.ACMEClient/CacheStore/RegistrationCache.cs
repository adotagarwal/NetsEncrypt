using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NetsEncrypt.ACMEClient.JsonWebSignature;
using NetsEncrypt.ACMEClient.Model;
using Newtonsoft.Json;

namespace NetsEncrypt.ACMEClient.CacheStore
{
    internal class RegistrationCache
    {
        #region Persistence Helpers

        public static void SaveCacheToFile(RegistrationCache cache, string fileName)
        {
            File.WriteAllText(fileName, JsonConvert.SerializeObject(cache, Newtonsoft.Json.Formatting.Indented));
        }

        public static RegistrationCache LoadCacheFromFile(string fileName)
        {
            return JsonConvert.DeserializeObject<RegistrationCache>(File.ReadAllText(fileName));
        }

        #endregion

        #region Singleton Instance

        private static readonly object _locker = new object();
        private static volatile RegistrationCache _cache;

        public static RegistrationCache Instance => _cache;
        
        public static void SetInstance(RegistrationCache cache)
        {
            if (_cache != null) return;

            lock (_locker)
                if (_cache ==null)
                    _cache = cache;
        }

        public static void SaveInstance(string fileName)
        {
            lock (_locker)
                SaveCacheToFile(Instance, fileName);
        }
        
        public static string GetDefaultCacheFilename(string apiUrl)
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);
            var hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(apiUrl));
            var file = JsonWebSigner.Base64UrlEncoded(hash) + ".lets-encrypt.cache.json";
            return Path.Combine(home, file);
        }

        public static void ResetCachedCertificate(IEnumerable<string> hostsToRemove)
        {
            foreach (var host in hostsToRemove)
                Instance?.CachedCerts?.Remove(host);
        }

        public static bool TryGetCachedCertificate(List<string> hosts, out ACMECertificate value)
        {
            if (Instance.CachedCerts.TryGetValue(hosts[0], out value) == false)
                return false;

            if ((value.Certificate.NotAfter - DateTime.UtcNow).TotalDays < 14)
                return false;

            return true;
        }

        public static void AddOrUpdateCachedCertificate(string host, ACMECertificate certificate)
        {
            lock (_locker)
            {
                Instance.CachedCerts[host] = certificate;
            }
        }

        #endregion


        public readonly Dictionary<string, ACMECertificate> CachedCerts = new Dictionary<string, ACMECertificate>(StringComparer.OrdinalIgnoreCase);

        public byte[] AccountKey { get; set; }
        public string Id { get; set; }
        public JsonWebKey Key { get; set; }
        public Uri Location { get; set; }
    }
}
