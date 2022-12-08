using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RimMod.Synchronization
{
    public sealed class ErrorHandlingSynchronizationManager : IItemSynchronizationManager
    {
        private readonly IItemSynchronizationManager _inner;
        private readonly ILogger _logger;

        public ErrorHandlingSynchronizationManager(
            IItemSynchronizationManager inner,
            ILogger logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public async Task UpdateAsync(IItem oldItem, IItem newItem,
            CancellationToken cancellationToken)
        {
            try
            {
                await _inner.UpdateAsync(oldItem, newItem, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("Updated item '{itemId}:{itemName}'", newItem.Id, newItem.Name);
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to update '{itemId}:{itemName}', {error}", newItem.Id, newItem.Name, e.Message);
            }
        }

        public async Task CreateAsync(IItem newItem, CancellationToken cancellationToken)
        {
            try
            {
                await _inner.CreateAsync(newItem, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("Added item '{itemId}:{itemName}'", newItem.Id, newItem.Name);
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to add '{itemId}:{itemName}', {error}", newItem.Id, newItem.Name, e.Message);
            }
        }

        public async Task DeleteAsync(IItemId itemId, CancellationToken cancellationToken)
        {
            try
            {
                await _inner.DeleteAsync(itemId, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("Removed item '{itemId}'", itemId);
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to remove '{itemId}', {error}", itemId, e.Message);
            }
        }
    }
}