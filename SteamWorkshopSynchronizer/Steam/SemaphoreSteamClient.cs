using System.Threading;
using System.Threading.Tasks;

namespace SteamWorkshopSynchronizer.Steam
{
    public sealed class SemaphoreSteamClient : ISteamClient
    {
        private readonly ISteamClient _inner;
        private readonly SemaphoreSlim _sync;

        public SemaphoreSteamClient(ISteamClient steamClientImplementation, int maxParallelCalls)
        {
            _inner = steamClientImplementation;
            _sync = new SemaphoreSlim(maxParallelCalls);
        }

        public async Task<string> DownloadWorkshopItemAndReturnPathAsync(int appId, long fileId, CancellationToken ct)
        {
            await _sync.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                return await _inner.DownloadWorkshopItemAndReturnPathAsync(appId, fileId, ct).ConfigureAwait(false);
            }
            finally
            {
                _sync.Release();
            }
        }
    }
}