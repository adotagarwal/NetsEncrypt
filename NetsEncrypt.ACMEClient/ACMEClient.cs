using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetsEncrypt.ACMEClient.ACME;
using NetsEncrypt.ACMEClient.CacheStore;
using NetsEncrypt.ACMEClient.JsonWebSignature;
using NetsEncrypt.ACMEClient.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetsEncrypt.ACMEClient
{

    public class ACMEClient : IACMEClient
    {
        public const string STAGE_API_ENDPOINT = "https://acme-staging-v02.api.letsencrypt.org/directory";
        public const string API_ENDPOINT = "https://acme-v02.api.letsencrypt.org/directory";

        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };
        
        #region Cache
        private readonly string _cachePath;
        #endregion

        #region Current State
        private APIDirectory _directory;
        private JsonWebSigner _encryptor;
        private RSACryptoServiceProvider _accountKey;
        private List<AuthorizationChallenge> _challenges = new List<AuthorizationChallenge>();
        private Order _currentOrder;
        private string _nonce;
        #endregion

        #region Immutables
        private HttpClient _client;
        #endregion

        public ACMEClient(string url)
        {
            //_url = url ?? throw new ArgumentNullException(nameof(url));
            _client = HttpClientCache.GetCachedClient(url ?? throw new ArgumentNullException(nameof(url)));
            _cachePath = RegistrationCache.GetDefaultCacheFilename(url);
        }

        public async Task Init(string email, CancellationToken token = default(CancellationToken))
        {
            _accountKey = new RSACryptoServiceProvider(4096);
            

            // retrieve the API directory
            (_directory, _) = await SendAsync<APIDirectory>(HttpMethod.Get, new Uri("directory", UriKind.Relative), null, token);

            // check for account
            if (File.Exists(_cachePath))
            {
                bool success;
                try
                {
                    RegistrationCache.SetInstance(RegistrationCache.LoadCacheFromFile(_cachePath));
                    _accountKey.ImportCspBlob(RegistrationCache.Instance.AccountKey);
                    _encryptor = new JsonWebSigner(_accountKey, RegistrationCache.Instance.Location.ToString());
                    success = true;
                }
                catch
                {
                    success = false;
                }

                if (success)
                    return;
            }

            // no account found, create a new account
            _encryptor = new JsonWebSigner(_accountKey, null);
            var (account, response) = await SendAsync<Account>(HttpMethod.Post, _directory.NewAccount, new Account
            {
                // we validate this in the UI before we get here, so that is fine
                TermsOfServiceAgreed = true,
                Contacts = new[] { "mailto:" + email },
            }, token);
            _encryptor.SetKeyId(account);

            if (account.Status != "valid")
                throw new InvalidOperationException("Account status is not valid, was: " + account.Status + Environment.NewLine + response);

            RegistrationCache.SetInstance(new RegistrationCache
            {
                Location = account.Location,
                AccountKey = _accountKey.ExportCspBlob(true),
                Id = account.Id,
                Key = account.Key
            });

            RegistrationCache.SaveInstance(_cachePath);
        }

        private async Task<(TResult Result, string Response)> SendAsync<TResult>(HttpMethod method, Uri uri, object message, CancellationToken token) where TResult : class
        {
            var request = new HttpRequestMessage(method, uri);

            // get a nonce for non-directory requests
            if (string.IsNullOrEmpty(_nonce) && uri.ToString() != new Uri(API_ENDPOINT).ToString())
            {
                _nonce = await GetNewNonce(token);
            }

            if (message != null || method == HttpMethod.Post)
            {
                var encodedMessage = _encryptor.Encode(message, new ACMERequestHeader
                {
                    Nonce = _nonce,
                    Url = uri,
                    KeyId = RegistrationCache.Instance?.Location?.ToString()
                });
                var json = JsonConvert.SerializeObject(encodedMessage, jsonSettings);

                request.Content = new StringContent(json, Encoding.UTF8, "application/jose+json")
                {
                    Headers = { ContentType = { CharSet = string.Empty } }
                };
            }

            var response = await _client.SendAsync(request, token).ConfigureAwait(false);

            if (response.Headers.Contains("Replay-Nonce"))
                _nonce = response.Headers.GetValues("Replay-Nonce").First();

            if (response.Content.Headers.ContentType.MediaType == "application/problem+json")
            {
                var problemJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var problem = JsonConvert.DeserializeObject<Problem>(problemJson);
                problem.RawJson = problemJson;
                throw new ACMEException(problem, response);
            }

            var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (typeof(TResult) == typeof(string)
                && response.Content.Headers.ContentType.MediaType == "application/pem-certificate-chain")
            {
                return ((TResult)(object)responseText, null);
            }

            var responseContent = JObject.Parse(responseText).ToObject<TResult>();

            if (responseContent is IHasLocation ihl)
            {
                if (response.Headers.Location != null)
                    ihl.Location = response.Headers.Location;
            }

            return (responseContent, responseText);
        }

        private async Task<string> GetNewNonce(CancellationToken token)
        {
            if (_directory == null)
                return null;

            var request = new HttpRequestMessage(HttpMethod.Head, _directory.NewNonce);

            var response = await _client.SendAsync(request, token).ConfigureAwait(false);

            if (response.Headers.Contains("Replay-Nonce"))
                return response.Headers.GetValues("Replay-Nonce").First();

            return null;
        }

        public async Task<Dictionary<string, string>> NewOrder(string[] hostnames, CancellationToken token = default(CancellationToken))
        {
            _challenges.Clear();
            var (order, response) = await SendAsync<Order>(HttpMethod.Post, _directory.NewOrder, new Order
            {
                Expires = DateTime.UtcNow.AddDays(2),
                Identifiers = hostnames.Select(hostname => new OrderIdentifier
                {
                    Type = "dns",
                    Value = hostname
                }).ToArray()
            }, token);

            if (order.Status != "pending")
            {
                if (order.Status != "ready")
                {
                    throw new InvalidOperationException("Created new order and expected status 'pending', but got: " + order.Status + Environment.NewLine + response);
                }
                else
                {
                    _currentOrder = order;
                    return new Dictionary<string, string>();
                }
            }

            _currentOrder = order;
            var results = new Dictionary<string, string>();
            foreach (var item in order.Authorizations)
            {
                _nonce = null;
                var (challengeResponse, responseText) = await SendAsync<AuthorizationChallengeResponse>(HttpMethod.Post, item, null, token);
                if (challengeResponse.Status == "valid")
                    continue;

                if (challengeResponse.Status != "pending")
                    throw new InvalidOperationException("Expected autorization status 'pending', but got: " + order.Status +
                        Environment.NewLine + responseText);

                var challenge = challengeResponse.Challenges.First(x => x.Type == "dns-01");
                _challenges.Add(challenge);
                var keyToken = _encryptor.GetKeyAuthorization(challenge.Token);
                using (var sha256 = SHA256.Create())
                {
                    var dnsToken = JsonWebSigner.Base64UrlEncoded(sha256.ComputeHash(Encoding.UTF8.GetBytes(keyToken)));
                    results[challengeResponse.Identifier.Value] = dnsToken;
                }
            }

            return results;
        }

        public async Task CompleteChallenges(CancellationToken token = default(CancellationToken))
        {
            // reset any nonce
            _nonce = null;
            for (var index = 0; index < _challenges.Count; index++)
            {
                var challenge = _challenges[index];

                while (true)
                {
                    var (result, responseText) = await SendAsync<AuthorizationChallengeResponse>(HttpMethod.Post, challenge.Url, new AuthorizeChallenge
                    {
                        KeyAuthorization = _encryptor.GetKeyAuthorization(challenge.Token)
                    }, token);

                    if (result.Status == "valid")
                        break;
                    if (result.Status != "pending")
                        throw new InvalidOperationException("Failed autorization of " + _currentOrder.Identifiers[index].Value + Environment.NewLine + responseText);

                    await Task.Delay(10000);
                }
            }
        }

        public async Task<ACMECertificate> GetCertificate(CancellationToken token = default(CancellationToken))
        {
            var key = new RSACryptoServiceProvider(4096);
            var csr = new CertificateRequest("CN=" + _currentOrder.Identifiers[0].Value,
                key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            var san = new SubjectAlternativeNameBuilder();
            foreach (var host in _currentOrder.Identifiers)
                san.AddDnsName(host.Value);

            csr.CertificateExtensions.Add(san.Build());

            var (response, responseText) = await SendAsync<Order>(HttpMethod.Post, _currentOrder.Finalize, new FinalizeRequest
            {
                CSR = JsonWebSigner.Base64UrlEncoded(csr.CreateSigningRequest())
            }, token);

            while (response.Status != "valid")
            {
                (response, responseText) = await SendAsync<Order>(HttpMethod.Get, response.Location, null, token);

                if (response.Status == "processing")
                {
                    await Task.Delay(500);
                    continue;
                }
                throw new InvalidOperationException("Invalid order status: " + response.Status + Environment.NewLine +
                    responseText);
            }
            var (pem, _) = await SendAsync<string>(HttpMethod.Post, response.Certificate, null, token);

            var retVal = new ACMECertificate() { CertificateString = pem, PrivateKey = key };

            RegistrationCache.AddOrUpdateCachedCertificate(_currentOrder.Identifiers[0].Value, retVal);
            RegistrationCache.SaveInstance(_cachePath);

            return retVal;
        }

        public Task RevokeCertificate(string[] domainIdentifiers, CancellationToken token = default(CancellationToken))
        {
            //todo: implement
            throw new NotImplementedException();
        }

        public string GetTermsOfServiceUri()
        {
            return _directory.Meta.TermsOfService;
        }

    }

}
