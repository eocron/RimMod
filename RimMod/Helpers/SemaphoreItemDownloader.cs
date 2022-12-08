using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RimMod.OnlineDownloaders;
using RimMod.Synchronization;

namespace RimMod.Helpers
{
    public sealed class SemaphoreItemDownloader : IItemDownloader
    {
        private readonly IItemDownloader _inner;
        private readonly SemaphoreSlim _semaphore;

        public SemaphoreItemDownloader(IItemDownloader inner, int maxParallelCount)
        {
            _inner = inner;
            _semaphore = new SemaphoreSlim(maxParallelCount);
        }

        public IReadOnlySet<Type> SupportedItemTypes => _inner.SupportedItemTypes;

        public async Task DownloadIntoFolderAsync(string folder, IItem item, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await _inner.DownloadIntoFolderAsync(folder, item, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}