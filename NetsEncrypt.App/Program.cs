using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using NetsEncrypt.Common;
using NetsEncrypt.ACMEClient;
using NetsEncrypt.WindowsDnsUpdater;

namespace NetsEncrypt.App
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

            // get an IDnsUpdater
            IDnsUpdater dnsUpdater = new PowerShellWindowsDnsUpdater();

            // get a client with the specified account
            IACMEClient client = new ACMEWebAPIClient(ACMEWebAPIClient.STAGE_API_ENDPOINT);
            
            // get a wrapper which will automate the process of obtaining a certificate
            var wrapper = new CertificateGenerator(client, dnsUpdater);

            var x = await wrapper.SaveNewWildCardCertificate(userAccount, domainToGenerate, path);
            Console.WriteLine($"Saved certificate to: {x}");
        }
    }
}
