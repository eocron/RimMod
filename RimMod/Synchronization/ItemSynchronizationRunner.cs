using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RimMod.Helpers;
using RimMod.Settings;

namespace RimMod.Synchronization
{
    public sealed class ItemSynchronizationRunner : IItemSynchronizationRunner
    {
        private readonly IRawItemProvider _sourceProvider;
        private readonly IRawItemProvider _targetProvider;
        private readonly IItemSynchronizationManager _synchronizationManager;
        private readonly UpdateMode _mode;

        public ItemSynchronizationRunner(
            IRawItemProvider sourceProvider,
            IRawItemProvider targetProvider,
            IItemSynchronizationManager synchronizationManager,
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
                .FullOuterJoin(sourceItems, x => x.Id, x => x.Id, (o, n, k) => new { o, n, k })
                .Select(async obj =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (obj.o != null && obj.n != null) //update
                    {
                        if (_mode.HasFlag(UpdateMode.Update))
                        {
                            if (obj.o.Version != obj.n.Version)
                            {
                                await _synchronizationManager.UpdateAsync(obj.o, obj.n, cancellationToken)
                                    .ConfigureAwait(false);
                            }
                        }
                    }
                    else if (obj.o == null && obj.n != null) //create
                    {
                        if (_mode.HasFlag(UpdateMode.Create))
                        {
                            await _synchronizationManager.CreateAsync(obj.n, cancellationToken)
                                .ConfigureAwait(false);
                        }
                    }
                    else if (obj.o != null && obj.n == null) //delete
                    {
                        if (_mode.HasFlag(UpdateMode.Delete))
                        {
                            await _synchronizationManager.DeleteAsync(obj.k, cancellationToken)
                                .ConfigureAwait(false);
                        }
                    }
                });
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}