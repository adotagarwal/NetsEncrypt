using LetsEncryptClient.DNS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace LetsEncryptClient
{
    class Program
    {
        /// <summary>
        /// this method produces a valid SSL certificate given an account email and a domain 
        /// </summary>
        /// <returns></returns>
        public static async Task Main()
        {
            var path = Path.GetTempPath();
            var userAccount = "johndoe@testdomain.com";
            var domainToGenerate = "testdomain.com";

            var x = await SaveNewWildCardCertificate(userAccount, domainToGenerate, path);
            Console.WriteLine($"Saved certificate to: {x}");
        }

        /// <summary>
        /// This method shows you how to string together the API to produce a valid SSL certificate given an account email and a domain 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="domainSuffix"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        static async Task<string> SaveNewWildCardCertificate(string account, string domainSuffix, string path)
        {
            // get an IDnsUpdater
            IDnsUpdater dnsUpdater = new PowerShellWindowsDnsUpdater();
            
            // get a client with the specified account
            IACMEClient client = new ACMEClient(ACMEClient.STAGE_API_ENDPOINT);
            await client.Init(account, CancellationToken.None);

            // the use of an ACME providers service implies agreement with their terms
            var tos = client.GetTermsOfServiceUri(); 

            // start a new order, by specifying the domains that the order should encompass
            Dictionary<string, string> challenges = await client.NewOrder(new[] { $"*.{domainSuffix}", $"{domainSuffix}" });

            // call out to have DNS records updated per the challenges
            foreach (var challenge in challenges)
                await dnsUpdater.CreateOrUpdateTXTRecord(challenge.Key, "_acme-challenge", challenge.Value);
            
            // wait a few seconds for the DNS to propagate 
            await Task.Delay(10000);

            // Now that the DNS is updated, invoke challenge completion at ACME endpoint
            await client.CompleteChallenges();

            // get the certificate for the successful order
            var cert = await client.GetCertificate();
            
            //combine public cert with the private key for a full pfx
            var pfx = cert.Certificate.CopyWithPrivateKey(cert.PrivateKey);
            var fn = Path.Combine(path, $"{domainSuffix}.wildcard.pfx");
            File.WriteAllBytes(fn, pfx.Export(X509ContentType.Pfx));

            // return generated filename
            return fn;
        }

        static async Task<string> SaveNewDomainCertificate(string account, string domainSuffix, string path)
        {
            // get an IDnsUpdater
            var dnsUpdater = new PowerShellWindowsDnsUpdater();

            // get a client with the specified account
            var client = new ACMEClient(ACMEClient.API_ENDPOINT);
            await client.Init(account, CancellationToken.None);

            // the use of an ACME providers service implies agreement with their terms
            var tos = client.GetTermsOfServiceUri(); // user should agree to this

            // start a new order, by specifying the domains that the order should encompass
            Dictionary<string, string> challenges = await client.NewOrder(new[] { $"{domainSuffix}" });

            // call out to have DNS records updated per the challenges
            foreach (var challenge in challenges)
                await dnsUpdater.CreateOrUpdateTXTRecord(challenge.Key, "_acme-challenge", challenge.Value);

            // Now that the DNS is updated, invoke challenge completion at ACME endpoint
            await client.CompleteChallenges();

            // get the certificate for the successful order
            var cert = await client.GetCertificate();

            //combine public cert with the private key for a full pfx
            var pfx = cert.Certificate.CopyWithPrivateKey(cert.PrivateKey);
            var fn = Path.Combine(path, $"{domainSuffix}.pfx");
            File.WriteAllBytes(fn, pfx.Export(X509ContentType.Pfx));

            // return generated filename
            return fn;
        }
    }
}
