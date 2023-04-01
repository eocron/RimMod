using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SteamWorkshopSynchronizer.Core
{
    public sealed class ErrorSuppressingTableEntityManager<T> : ITableEntityManager<T> where T : ITableEntity
    {
        private readonly ITableEntityManager<T> _inner;

        public ErrorSuppressingTableEntityManager(ITableEntityManager<T> inner)
        {
            _inner = inner;
        }

        public async Task DeleteEntityAsync(string key, CancellationToken ct)
        {
            try
            {
                await _inner.DeleteEntityAsync(key, ct).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        public async Task UpdateEntityAsync(T entity, CancellationToken ct)
        {
            try
            {
                await _inner.UpdateEntityAsync(entity, ct).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        public async Task CreateEntityAsync(T entity, CancellationToken ct)
        {
            try
            {
                await _inner.CreateEntityAsync(entity, ct).ConfigureAwait(false);
            }
            catch
            {
            }
        }
    }
}