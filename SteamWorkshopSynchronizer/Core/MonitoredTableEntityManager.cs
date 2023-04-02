using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SteamWorkshopSynchronizer.Core
{
    public class MonitoredTableEntityManager<T> : ITableEntityManager<T> where T : IFileTableEntity
    {
        private readonly ITableEntityManager<T> _inner;
        private readonly ILogger _logger;

        public MonitoredTableEntityManager(ITableEntityManager<T> inner, ILogger logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public async Task DeleteEntityAsync(string key, CancellationToken ct)
        {
            try
            {
                await _inner.DeleteEntityAsync(key, ct).ConfigureAwait(false);
                _logger.LogInformation("Deleted {key}", key);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        public async Task UpdateEntityAsync(T entity, CancellationToken ct)
        {
            try
            {
                await _inner.UpdateEntityAsync(entity, ct).ConfigureAwait(false);
                _logger.LogInformation("Updated {title}", entity.EscapedTitle);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        public async Task CreateEntityAsync(T entity, CancellationToken ct)
        {
            try
            {
                await _inner.CreateEntityAsync(entity, ct).ConfigureAwait(false);
                _logger.LogInformation("Created {title}", entity.EscapedTitle);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }
    }
}