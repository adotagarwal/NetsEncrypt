using System.Threading.Tasks;

namespace NetsEncrypt.ACMEClient.DNS
{
    public interface IDnsUpdater
    {
        Task CreateOrUpdateTXTRecord(string zone, string host, string text);
    }
}
