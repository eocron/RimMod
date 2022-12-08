using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using RimMod.OnlineDownloaders;
using RimMod.Synchronization;

namespace RimMod.Helpers
{
    public sealed class InfiniteRetryItemDownloader : IItemDownloader
    {
        private readonly IItemDownloader _inner;
        private readonly AsyncRetryPolicy _policy;

        public InfiniteRetryItemDownloader(IItemDownloader inner, TimeSpan waitInterval)
        {
            _inner = inner;
            _policy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryForeverAsync((i, ctx) => waitInterval);
        }

        public IReadOnlySet<Type> SupportedItemTypes => _inner.SupportedItemTypes;

        public async Task DownloadIntoFolderAsync(string folder, IItem item, CancellationToken cancellationToken)
        {
            await _policy
                .ExecuteAsync(
                    ct => _inner.DownloadIntoFolderAsync(folder, item, ct), 
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}