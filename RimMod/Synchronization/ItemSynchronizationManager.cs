using System.Threading;
using System.Threading.Tasks;
using RimMod.OnlineDownloaders;

namespace RimMod.Synchronization
{
    public sealed class ItemSynchronizationManager : IItemSynchronizationManager
    {
        private readonly IItemManager _targetManager;
        private readonly IItemDownloader _downloader;

        public ItemSynchronizationManager(IItemManager targetManager, IItemDownloader downloader)
        {
            _targetManager = targetManager;
            _downloader = downloader;
        }

        public async Task UpdateAsync(IItem oldItem, IItem newItem, CancellationToken cancellationToken)
        {
            var folder = _targetManager.GetFolder(newItem.GetFolderName());
            await _downloader.DownloadIntoFolderAsync(folder, newItem, cancellationToken).ConfigureAwait(false);
            await _targetManager.AddItemAsync(newItem, cancellationToken).ConfigureAwait(false);
        }

        public async Task CreateAsync(IItem newItem, CancellationToken cancellationToken)
        {
            var folder = _targetManager.GetFolder(newItem.GetFolderName());
            await _downloader.DownloadIntoFolderAsync(folder, newItem, cancellationToken).ConfigureAwait(false);
            await _targetManager.AddItemAsync(newItem, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(IItemId itemId, CancellationToken cancellationToken)
        {
            await _targetManager.DeleteAsync(itemId, cancellationToken).ConfigureAwait(false);
        }
    }
}