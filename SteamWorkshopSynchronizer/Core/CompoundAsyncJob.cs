using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SteamWorkshopSynchronizer.Core
{
    public sealed class CompoundAsyncJob : IAsyncJob
    {
        private readonly bool _inParallel;
        private readonly IAsyncJob[] _jobs;

        public CompoundAsyncJob(bool inParallel, params IAsyncJob[] jobs)
        {
            _inParallel = inParallel;
            _jobs = jobs ?? throw new ArgumentNullException(nameof(jobs));
        }
        public async Task RunAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (_inParallel)
            {
                await Task.WhenAll(_jobs.Select(x => x.RunAsync(ct))).ConfigureAwait(false);
            }
            else
            {
                foreach (var job in _jobs)
                {
                    ct.ThrowIfCancellationRequested();
                    await job.RunAsync(ct).ConfigureAwait(false);
                }
            }
        }
    }
}