using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace NetsEncrypt.Common
{
    public class CertificateGenerator
    {
        private readonly IACMEClient _acmeClient;
        private readonly IDnsUpdater _dnsUpdater;

        public CertificateGenerator(IACMEClient acmeClient, IDnsUpdater dnsUpdater)
        {
            _acmeClient = acmeClient;
            _dnsUpdater = dnsUpdater;
        }

        /// <summary>
        /// This method shows you how to string together the API to produce a valid SSL certificate given an account email and a domain 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="domainSuffix"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<string> SaveNewWildCardCertificate(string account, string domainSuffix, string path)
        {
            // init the client
            await _acmeClient.Init(account, CancellationToken.None);

            // the use of an ACME providers service implies agreement with their terms
            var tos = _acmeClient.GetTermsOfServiceUri();

            // start a new order, by specifying the domains that the order should encompass
            Dictionary<string, string> challenges = await _acmeClient.NewOrder(new[] { $"*.{domainSuffix}", $"{domainSuffix}" });

            // call out to have DNS records updated per the challenges
            foreach (var challenge in challenges)
                await _dnsUpdater.CreateOrUpdateTXTRecord(challenge.Key, "_acme-challenge", challenge.Value);

            // wait a few seconds for the DNS to propagate 
            await Task.Delay(10000);

            // Now that the DNS is updated, invoke challenge completion at ACME endpoint
            await _acmeClient.CompleteChallenges();

            // get the certificate for the successful order
            var cert = await _acmeClient.GetCertificate();

            //combine public cert with the private key for a full pfx
            var pfx = cert.Certificate.CopyWithPrivateKey(cert.PrivateKey);
            var fn = Path.Combine(path, $"{domainSuffix}.wildcard.pfx");
            File.WriteAllBytes(fn, pfx.Export(X509ContentType.Pfx));

            // return generated filename
            return fn;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="domainSuffix"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<string> SaveNewDomainCertificate(string account, string domainSuffix, string path)
        {
            // initialize the client with the specified account
            await _acmeClient.Init(account, CancellationToken.None);

            // the use of an ACME providers service implies agreement with their terms
            var tos = _acmeClient.GetTermsOfServiceUri(); // user should agree to this

            // start a new order, by specifying the domains that the order should encompass
            Dictionary<string, string> challenges = await _acmeClient.NewOrder(new[] { $"{domainSuffix}" });

            // call out to have DNS records updated per the challenges
            foreach (var challenge in challenges)
                await _dnsUpdater.CreateOrUpdateTXTRecord(challenge.Key, "_acme-challenge", challenge.Value);

            // Now that the DNS is updated, invoke challenge completion at ACME endpoint
            await _acmeClient.CompleteChallenges();

            // get the certificate for the successful order
            var cert = await _acmeClient.GetCertificate();

            //combine public cert with the private key for a full pfx
            var pfx = cert.Certificate.CopyWithPrivateKey(cert.PrivateKey);
            var fn = Path.Combine(path, $"{domainSuffix}.pfx");
            File.WriteAllBytes(fn, pfx.Export(X509ContentType.Pfx));

            // return generated filename
            return fn;
        }
    }
}