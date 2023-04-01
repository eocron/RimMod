using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SteamWorkshopSynchronizer.Commands;

namespace SteamWorkshopSynchronizer.Steam
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
            if (_inParallel)
            {
                await Task.WhenAll(_jobs.Select(x => x.RunAsync(ct))).ConfigureAwait(false);
            }
            else
            {
                foreach (var job in _jobs)
                {
                    await job.RunAsync(ct).ConfigureAwait(false);
                }
            }
        }
    }
}