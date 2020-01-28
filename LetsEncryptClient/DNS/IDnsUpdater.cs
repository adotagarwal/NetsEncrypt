using System.Threading.Tasks;

namespace LetsEncryptClient.DNS
{
    public interface IDnsUpdater
    {
        Task CreateOrUpdateTXTRecord(string zone, string host, string text);
    }
}
