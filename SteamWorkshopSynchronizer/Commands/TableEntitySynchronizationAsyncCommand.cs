using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SteamWorkshopSynchronizer.Settings;

namespace SteamWorkshopSynchronizer
{
    public sealed class TableEntitySynchronizationAsyncCommand<TEntityInfo> : IAsyncCommand
        where TEntityInfo : class, ITableEntity
    {
        private readonly ITableEntityProvider<TEntityInfo> _sourceProvider;
        private readonly ITableEntityProvider<TEntityInfo> _targetProvider;
        private readonly ITableEntityManager<TEntityInfo> _targetManager;
        private readonly SynchronizationMode _mode;
        private readonly ILogger _logger;

        public TableEntitySynchronizationAsyncCommand(
            ITableEntityProvider<TEntityInfo> sourceProvider,
            ITableEntityProvider<TEntityInfo> targetProvider,
            ITableEntityManager<TEntityInfo> targetManager,
            SynchronizationMode mode,
            ILogger logger)
        {
            _sourceProvider = sourceProvider;
            _targetProvider = targetProvider;
            _targetManager = targetManager;
            _mode = mode;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken ct)
        {
            var sourceEntities = await _sourceProvider.GetAllEntitiesAsync(ct).ConfigureAwait(false);
            var targetEntities = await _targetProvider.GetAllEntitiesAsync(ct).ConfigureAwait(false);

            var fjoin = sourceEntities.FullOuterJoinDistinct(targetEntities, s => s.Key, t => t.Key,
                    (source, target) => new { source, target })
                .ToLookup(x => GetMode(x.source, x.target));

            var toDelete = (_mode & SynchronizationMode.Delete) != 0
                ? fjoin[SynchronizationMode.Delete].Select(x => x.target.Key).ToArray()
                : Array.Empty<string>();
            var toCreate = (_mode & SynchronizationMode.Create) != 0
                ? fjoin[SynchronizationMode.Create].Select(x => x.source).ToArray()
                : Array.Empty<TEntityInfo>();
            var toUpdate = (_mode & SynchronizationMode.Update) != 0
                ? fjoin[SynchronizationMode.Update].Select(x => x.source).ToArray()
                : Array.Empty<TEntityInfo>();

            _logger.LogInformation("ToDelete: {toDelete}, ToCreate: {toCreate}, ToUpdate: {toUpdate}", toDelete.Length,
                toCreate.Length, toUpdate.Length);

            foreach (var tableEntity in toCreate)
            {
                await _targetManager.CreateEntityAsync(tableEntity, ct).ConfigureAwait(false);
            }

            foreach (var tableEntity in toUpdate)
            {
                await _targetManager.UpdateEntityAsync(tableEntity, ct).ConfigureAwait(false);
            }

            foreach (var key in toDelete)
            {
                await _targetManager.DeleteEntityAsync(key, ct);
            }
        }

        private SynchronizationMode? GetMode(TEntityInfo source, TEntityInfo target)
        {
            if (source == null && target != null)
            {
                return SynchronizationMode.Delete;
            }

            if (source != null && target != null)
            {
                if (source.Modified != target.Modified)
                {
                    return SynchronizationMode.Update;
                }

                return null;
            }

            if (source != null && target == null)
            {
                return SynchronizationMode.Create;
            }

            throw new InvalidOperationException("Not possible to source and target be null at the same time");
        }
    }
}