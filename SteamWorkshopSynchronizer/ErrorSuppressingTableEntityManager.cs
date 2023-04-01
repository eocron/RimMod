using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SteamWorkshopSynchronizer
{
    public sealed class ErrorSuppressingTableEntityManager<T> : ITableEntityManager<T> where T : ITableEntity
    {
        private readonly ITableEntityManager<T> _inner;
        private readonly ILogger _logger;

        public ErrorSuppressingTableEntityManager(ITableEntityManager<T> inner, ILogger logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public async Task DeleteEntityAsync(string key, CancellationToken ct)
        {
            try
            {
                await _inner.DeleteEntityAsync(key, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        public async Task UpdateEntityAsync(T entity, CancellationToken ct)
        {
            try
            {
                await _inner.UpdateEntityAsync(entity, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        public async Task CreateEntityAsync(T entity, CancellationToken ct)
        {
            try
            {
                await _inner.CreateEntityAsync(entity, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }
    }
}