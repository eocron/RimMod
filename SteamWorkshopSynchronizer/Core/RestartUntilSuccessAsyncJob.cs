using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SteamWorkshopSynchronizer.Core
{
    public sealed class RestartUntilSuccessAsyncJob : IAsyncJob
    {
        private readonly IAsyncJob _inner;
        private readonly ILogger _logger;
        private readonly TimeSpan _restartInterval;

        public RestartUntilSuccessAsyncJob(IAsyncJob inner, ILogger logger, TimeSpan restartInterval)
        {
            _inner = inner;
            _logger = logger;
            _restartInterval = restartInterval;
        }

        public async Task RunAsync(CancellationToken ct)
        {
            while (true)
            {
                try
                {
                    await _inner.RunAsync(ct).ConfigureAwait(false);
                    return;
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                }

                _logger.LogInformation("Restart after {interval}", _restartInterval);
                await Task.Delay(_restartInterval, ct).ConfigureAwait(false);
            }

        }
    }
}