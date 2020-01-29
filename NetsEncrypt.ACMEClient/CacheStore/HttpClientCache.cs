using System;
using System.Collections.Generic;
using System.Net.Http;

namespace NetsEncrypt.ACMEClient.CacheStore
{
    public static class HttpClientCache
    {
        private static Dictionary<string, HttpClient> _cachedClients = new Dictionary<string, HttpClient>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     In our scenario, we assume a single single wizard progressing
        ///     and the locking is basic to the wizard progress. Adding explicit
        ///     locking to be sure that we are not corrupting disk state if user
        ///     is explicitly calling stuff concurrently (running the setup wizard
        ///     from two tabs?)
        /// </summary>
        private static readonly object _locker = new object();

        public static HttpClient GetCachedClient(string url)
        {
            if (_cachedClients.TryGetValue(url, out var value))
            {
                return value;
            }

            lock (_locker)
            {
                if (_cachedClients.TryGetValue(url, out value))
                {
                    return value;
                }

                value = new HttpClient
                {
                    BaseAddress = new Uri(url)
                };

                _cachedClients = new Dictionary<string, HttpClient>(_cachedClients, StringComparer.OrdinalIgnoreCase)
                {
                    [url] = value
                };
                return value;
            }
        }
    }

}
