using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RimMod
{
    public interface ISteamModDetailsProvider
    {
        Task<ModDetails> GetRemoteDetails(long fileId, CancellationToken cancellationToken);
        Task<ModDetails> GetLocalDetails(string folder, CancellationToken cancellationToken);
        Task SaveLocalDetails(string folder, ModDetails details, CancellationToken cancellationToken);
    }
}