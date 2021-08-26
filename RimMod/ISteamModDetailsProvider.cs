using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RimMod
{
    public interface ISteamModDetailsProvider
    {
        Task<ModDetails> GetDetails(long fileId, CancellationToken cancellationToken);
        Task<ModDetails> GetDetails(string folder, CancellationToken cancellationToken);
        Task SaveDetails(string folder, ModDetails details, CancellationToken cancellationToken);
    }
}