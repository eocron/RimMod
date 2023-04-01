﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SteamWorkshopSynchronizer.Settings;

namespace SteamWorkshopSynchronizer.Core
{
    public sealed class TableEntitySynchronizationAsyncCommand<TEntityInfo> : IAsyncJob
        where TEntityInfo : class, IFileTableEntity
    {
        private readonly ITableEntityProvider<TEntityInfo> _sourceProvider;
        private readonly ITableEntityProvider<TEntityInfo> _targetProvider;
        private readonly ITableEntityManager<TEntityInfo> _targetManager;
        private readonly SynchronizationMode _mode;
        private readonly bool _inParallel;
        private readonly ILogger _logger;

        public TableEntitySynchronizationAsyncCommand(
            ITableEntityProvider<TEntityInfo> sourceProvider,
            ITableEntityProvider<TEntityInfo> targetProvider,
            ITableEntityManager<TEntityInfo> targetManager,
            SynchronizationMode mode,
            bool inParallel,
            ILogger logger)
        {
            _sourceProvider = sourceProvider;
            _targetProvider = targetProvider;
            _targetManager = targetManager;
            _mode = mode;
            _inParallel = inParallel;
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
                ? fjoin[SynchronizationMode.Delete].Select(x => x.target).ToArray()
                : Array.Empty<TEntityInfo>();
            var toCreate = (_mode & SynchronizationMode.Create) != 0
                ? fjoin[SynchronizationMode.Create].Select(x => x.source).ToArray()
                : Array.Empty<TEntityInfo>();
            var toUpdate = (_mode & SynchronizationMode.Update) != 0
                ? fjoin[SynchronizationMode.Update].Select(x => x.source).ToArray()
                : Array.Empty<TEntityInfo>();

            _logger.LogInformation("ToDelete: {toDelete}, ToCreate: {toCreate}, ToUpdate: {toUpdate}", toDelete.Length,
                toCreate.Length, toUpdate.Length);

            await RunForEachAsync(toCreate, async tableEntity =>
            {
                await _targetManager.CreateEntityAsync(tableEntity, ct).ConfigureAwait(false);
                _logger.LogInformation("Created {escapedTitle}", tableEntity.EscapedTitle);
            }).ConfigureAwait(false);
            await RunForEachAsync(toUpdate, async tableEntity =>
            {
                await _targetManager.UpdateEntityAsync(tableEntity, ct).ConfigureAwait(false);
                _logger.LogInformation("Updated {escapedTitle}", tableEntity.EscapedTitle);
            }).ConfigureAwait(false);
            await RunForEachAsync(toDelete, async tableEntity =>
            {
                await _targetManager.DeleteEntityAsync(tableEntity.Key, ct);
                _logger.LogInformation("Deleted {escapedTitle}", tableEntity.EscapedTitle);
            }).ConfigureAwait(false);
        }

        private async Task RunForEachAsync<T>(IEnumerable<T> items, Func<T, Task> action)
        {
            if (_inParallel)
            {
                await Task.WhenAll(items.Select(action)).ConfigureAwait(false);
            }
            else
            {
                foreach (var i in items)
                {
                    await action(i).ConfigureAwait(false);
                }
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
                if (source.ETag != target.ETag)
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