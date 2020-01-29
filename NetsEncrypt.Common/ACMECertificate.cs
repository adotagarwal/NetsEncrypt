using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;

namespace NetsEncrypt.Common
{
    public class ACMECertificate
    {
        [JsonIgnore]
        public RSA PrivateKey { get; set; }

        //TODO: switch impls, bytes should be read/write while RSA should be generated
        public byte[] PrivateKeyBytes
        {
            get => ((RSACryptoServiceProvider)PrivateKey).ExportCspBlob(true);
            set
            {
                var newRsa = new RSACryptoServiceProvider(4096);
                newRsa.ImportCspBlob(value);
                PrivateKey = newRsa;
            }
        }

        public string CertificateString { get; set; }

        [JsonIgnore]
        public X509Certificate2 Certificate 
        {
            get => new X509Certificate2(Encoding.UTF8.GetBytes(CertificateString));
            set => CertificateString = Encoding.UTF8.GetString(value.RawData);
        }
    }
}
