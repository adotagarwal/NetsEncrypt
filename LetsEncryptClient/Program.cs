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
        public static async Task Main()
        {
            var x = await SaveNewWildCardCertificate("testdomain.com");
            x.Dump();
        }

        static async Task<string> SaveNewWildCardCertificate(string domainSuffix, string path = @"c:\cert-root\")
        {
            var dnsUpdater = new PowerShellWindowsDnsUpdater();
            var client = new LetsEncryptClient(LetsEncryptClient.STAGE_API_ENDPOINT);
            await client.Init("johndoe@gmail.com", CancellationToken.None);
            var tos = client.GetTermsOfServiceUri(); // user should agree to this

            // start a new order for the *.example.net wildcard domain
            Dictionary<string, string> challenges = await client.NewOrder(new[] { $"*.{domainSuffix}", $"{domainSuffix}" });

            // do the DNS challenge
            foreach (var challenge in challenges)
            {
                await dnsUpdater.CreateOrUpdateTXTRecord(challenge.Key, "_acme-challenge", challenge.Value);
            }

            await Task.Delay(10000);

            // Now that the DNS is updated, let Let's Encrypt know that it can validate them
            await client.CompleteChallenges();

            // get the certificate for the successful order
            var cert = await client.GetCertificate();

            cert.Cert.Dump();
            cert.PrivateKey.Dump();

            //combine public cert with the private key for a full pfx
            var pfx = cert.Cert.CopyWithPrivateKey(cert.PrivateKey);
            var fn = Path.Combine(path, $"{domainSuffix}.wildcard.pfx");
            File.WriteAllBytes(fn, pfx.Export(X509ContentType.Pfx));
            return fn;
        }

        static async Task SaveNewDomainCertificate(string domainSuffix, string path = @"c:\cert-root\")
        {
            var dnsUpdater = new PowerShellWindowsDnsUpdater();
            var client = new LetsEncryptClient(LetsEncryptClient.API_ENDPOINT);
            await client.Init("johndoe@gmail.com", CancellationToken.None);
            var tos = client.GetTermsOfServiceUri(); // user should agree to this

            // start a new order for the *.example.net wildcard domain
            Dictionary<string, string> challenges = await client.NewOrder(new[] { $"{domainSuffix}" });

            // do the DNS challenge
            foreach (var challenge in challenges)
            {
                await dnsUpdater.CreateOrUpdateTXTRecord(challenge.Key, "_acme-challenge", challenge.Value);
            }

            // Now that the DNS is updated, let Let's Encrypt know that it can validate them
            await client.CompleteChallenges();

            // get the certificate for the successful order
            var cert = await client.GetCertificate();
            //combine public cert with the private key for a full pfx
            var pfx = cert.Cert.CopyWithPrivateKey(cert.PrivateKey);
            File.WriteAllBytes(Path.Combine(path, $"{domainSuffix}.pfx"), pfx.Export(X509ContentType.Pfx));
        }
    }
}
