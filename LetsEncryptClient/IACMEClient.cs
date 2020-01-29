using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using LetsEncryptClient.CacheStore;

namespace LetsEncryptClient
{
    public interface IACMEClient
    {
        /// <summary>
        /// Initialize an ACME Client with the specified account email address
        /// </summary>
        /// <param name="accountEmail"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task Init(string accountEmail, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Part of the draft spec specified agreeing to the terms of service
        /// </summary>
        /// <returns></returns>
        string GetTermsOfServiceUri();


        /// <summary>
        /// Create a new order for a set of domain identifiers. 
        /// </summary>
        /// <param name="domainIdentifiers"></param>
        /// <param name="token"></param>
        /// <returns>a dictionary of domain (key) to challenge responses (value)</returns>
        Task<Dictionary<string, string>> NewOrder(string[] domainIdentifiers, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Inform ACME Server that the challenges associated with the current order are ready to be checked
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task CompleteChallenges(CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Download the now issued certificate
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Public certificate and Private key</returns>
        Task<ACMECertificate> GetCertificate(CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Revokes certificates by searching our cache for a certificate identifier and hitting the revoke URL
        /// </summary>
        /// <param name="domainIdentifiers"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task RevokeCertificate(string[] domainIdentifiers, CancellationToken token = default(CancellationToken));

        
    }
}
