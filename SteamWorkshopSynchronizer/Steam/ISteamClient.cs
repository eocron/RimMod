using System.Threading;
using System.Threading.Tasks;

namespace SteamWorkshopSynchronizer.Steam
{
    public interface ISteamClient
    {
        Task<string> DownloadWorkshopItemAndReturnPathAsync(int appId, long fileId, CancellationToken ct);
    }
}