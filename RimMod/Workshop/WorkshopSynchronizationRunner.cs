using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RimMod.Settings;

namespace RimMod.Workshop
{
    public class WorkshopSynchronizationRunner : IWorkshopSynchronizationRunner
    {
        private readonly IWorkshopProvider _sourceProvider;
        private readonly IWorkshopProvider _targetProvider;
        private readonly IWorkshopSynchronizationManager _synchronizationManager;
        private readonly UpdateMode _mode;

        public WorkshopSynchronizationRunner(
            IWorkshopProvider sourceProvider,
            IWorkshopProvider targetProvider,
            IWorkshopSynchronizationManager synchronizationManager,
            UpdateMode mode)
        {
            _sourceProvider = sourceProvider;
            _targetProvider = targetProvider;
            _synchronizationManager = synchronizationManager;
            _mode = mode;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var sourceItems = await _sourceProvider.GetAllItemsAsync(cancellationToken).ConfigureAwait(false);
            var targetItems = await _targetProvider.GetAllItemsAsync(cancellationToken).ConfigureAwait(false);


            var tasks = targetItems
                .FullOuterJoin(sourceItems, x => x.ItemId, x => x.ItemId, (o, n, k) => new { o, n, k })
                .Select(async obj =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (obj.o != null && obj.n != null) //update
                    {
                        if (_mode.HasFlag(UpdateMode.Update))
                        {
                            if (obj.o.LastUpdatedTimestamp != obj.n.LastUpdatedTimestamp)
                            {
                                await _synchronizationManager.UpdateAsync(obj.o, obj.n, cancellationToken).ConfigureAwait(false);
                            }
                        }
                    }
                    else if (obj.o == null && obj.n != null) //create
                    {
                        if (_mode.HasFlag(UpdateMode.Create))
                        {
                            await _synchronizationManager.CreateAsync(obj.n, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    else if (obj.o != null && obj.n == null) //delete
                    {
                        if (_mode.HasFlag(UpdateMode.Delete))
                        {
                            await _synchronizationManager.DeleteAsync(obj.k, cancellationToken).ConfigureAwait(false);
                        }
                    }

                });
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}