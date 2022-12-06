using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using RimMod.OnlineDownloaders;
using RimMod.Workshop.Entities;

namespace RimMod.Helpers
{
    public sealed class InfiniteRetryWorkshopItemDownloader : IWorkshopItemDownloader
    {
        private readonly IWorkshopItemDownloader _inner;
        private readonly AsyncRetryPolicy _policy;

        public InfiniteRetryWorkshopItemDownloader(IWorkshopItemDownloader inner, TimeSpan waitInterval)
        {
            _inner = inner;
            _policy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryForeverAsync((i, ctx) => waitInterval);
        }

        public async Task DownloadIntoFolderAsync(string folder, WorkshopItemDetails details, CancellationToken cancellationToken)
        {
            await _policy
                .ExecuteAsync(
                    ct => _inner.DownloadIntoFolderAsync(folder, details, ct), 
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}