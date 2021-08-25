using System.Threading;
using System.Threading.Tasks;

namespace RimMod
{
    public interface ISteamModDownloader
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}