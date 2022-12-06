using System.Threading;
using System.Threading.Tasks;
using RimMod.Workshop.Entities;

namespace RimMod.OnlineDownloaders
{
    public sealed class SemaphoreWorkshopItemDownloader : IWorkshopItemDownloader
    {
        private readonly IWorkshopItemDownloader _inner;
        private readonly SemaphoreSlim _semaphore;

        public SemaphoreWorkshopItemDownloader(IWorkshopItemDownloader inner, int maxParallelCount)
        {
            _inner = inner;
            _semaphore = new SemaphoreSlim(maxParallelCount);
        }

        public async Task DownloadIntoFolderAsync(string folder, WorkshopItemDetails details, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await _inner.DownloadIntoFolderAsync(folder, details, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}