using System.Threading.Tasks;

namespace NetsEncrypt.Common
{
    public interface IDnsUpdater
    {
        Task CreateOrUpdateTXTRecord(string zone, string host, string text);
    }
}
