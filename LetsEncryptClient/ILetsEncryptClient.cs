using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace LetsEncryptClient
{
    public interface ILetsEncryptClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountEmail"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task Init(string accountEmail, CancellationToken token);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        string GetTermsOfServiceUri();

        /// <summary>
        /// This will give back a list of domain to challenge responses
        /// </summary>
        /// <param name="domainIdentifiers"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> NewOrder(string[] domainIdentifiers, CancellationToken token);

        /// <summary>
        /// This is used to indicate that challenges are ready to be checked
        /// </summary>
        /// <returns></returns>
        Task CompleteChallenges(CancellationToken token);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<(X509Certificate2 Cert, RSA PrivateKey)> GetCertificate(CancellationToken token);
    }
}
