using System.Threading;
using System.Threading.Tasks;

namespace SteamWorkshopSynchronizer.Steam
{
    public interface ISteamCmdClient
    {
        Task LoginAnonymousAsync(CancellationToken ct);
        Task<string> DownloadWorkshopItemAndReturnPathAsync(int appId, long fileId, CancellationToken ct);
    }
}