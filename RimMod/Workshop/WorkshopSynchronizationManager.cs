using System.Threading;
using System.Threading.Tasks;
using RimMod.OnlineDownloaders;
using RimMod.Workshop.Entities;

namespace RimMod.Workshop
{
    public sealed class WorkshopSynchronizationManager : IWorkshopSynchronizationManager
    {
        private readonly IWorkshopManager _targetManager;
        private readonly IWorkshopItemDownloader _downloader;

        public WorkshopSynchronizationManager(IWorkshopManager targetManager, IWorkshopItemDownloader downloader)
        {
            _targetManager = targetManager;
            _downloader = downloader;
        }

        public async Task UpdateAsync(WorkshopItemDetails oldItem, WorkshopItemDetails newItem, CancellationToken cancellationToken)
        {
            var folder = _targetManager.GetFolder(newItem.ItemId);
            await _downloader.DownloadIntoFolderAsync(folder, newItem, cancellationToken).ConfigureAwait(false);
        }

        public async Task CreateAsync(WorkshopItemDetails newItem, CancellationToken cancellationToken)
        {
            var folder = _targetManager.GetFolder(newItem.ItemId);
            await _downloader.DownloadIntoFolderAsync(folder, newItem, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(long itemId, CancellationToken cancellationToken)
        {
            await _targetManager.DeleteAsync(itemId, cancellationToken).ConfigureAwait(false);
        }
    }
}