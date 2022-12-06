using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RimMod.Workshop;
using RimMod.Workshop.Entities;

namespace RimMod.Helpers
{
    public sealed class LoggedSynchronizationManager : IWorkshopSynchronizationManager
    {
        private readonly IWorkshopSynchronizationManager _inner;
        private readonly ILogger<LoggedSynchronizationManager> _logger;

        public LoggedSynchronizationManager(
            IWorkshopSynchronizationManager inner,
            ILogger<LoggedSynchronizationManager> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public async Task UpdateAsync(WorkshopItemDetails oldItem, WorkshopItemDetails newItem,
            CancellationToken cancellationToken)
        {
            try
            {
                await _inner.UpdateAsync(oldItem, newItem, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation($"Updated item {oldItem.ItemId}");
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }

        public async Task CreateAsync(WorkshopItemDetails newItem, CancellationToken cancellationToken)
        {
            try
            {
                await _inner.CreateAsync(newItem, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation($"Added item {newItem.ItemId}");
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }

        public async Task DeleteAsync(long itemId, CancellationToken cancellationToken)
        {
            try
            {
                await _inner.DeleteAsync(itemId, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation($"Removed item {itemId}");
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }
    }
}