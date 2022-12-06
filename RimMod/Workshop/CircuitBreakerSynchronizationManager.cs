using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RimMod.Workshop.Entities;

namespace RimMod.Workshop
{
    public sealed class CircuitBreakerSynchronizationManager : IWorkshopSynchronizationManager
    {
        private readonly IWorkshopSynchronizationManager _inner;
        private readonly ILogger _logger;

        public CircuitBreakerSynchronizationManager(
            IWorkshopSynchronizationManager inner,
            ILogger logger)
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
                _logger.LogInformation("Updated item '{itemId}:{itemName}:{tags}'", newItem.ItemId, newItem.EscapedTitle, GetTags(newItem));
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to update '{itemId}:{itemName}:{tags}', {error}", newItem.ItemId, newItem.EscapedTitle, GetTags(newItem), e.Message);
            }
        }

        public async Task CreateAsync(WorkshopItemDetails newItem, CancellationToken cancellationToken)
        {
            try
            {
                await _inner.CreateAsync(newItem, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("Added item '{itemId}:{itemName}:{tags}'", newItem.ItemId, newItem.EscapedTitle, GetTags(newItem));
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to add '{itemId}:{itemName}:{tags}', {error}", newItem.ItemId, newItem.EscapedTitle, GetTags(newItem), e.Message);
            }
        }

        public async Task DeleteAsync(long itemId, CancellationToken cancellationToken)
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

        private static string GetTags(WorkshopItemDetails details)
        {
            return string.Join(",", details.Tags.Select(x => x.Tag));
        }
    }
}