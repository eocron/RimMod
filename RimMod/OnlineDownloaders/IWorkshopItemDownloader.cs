using System.Threading;
using System.Threading.Tasks;
using RimMod.Workshop.Entities;

namespace RimMod.OnlineDownloaders
{
    public interface IWorkshopItemDownloader
    {
        Task DownloadIntoFolderAsync(string folder, WorkshopItemDetails details, CancellationToken cancellationToken);
    }
}
