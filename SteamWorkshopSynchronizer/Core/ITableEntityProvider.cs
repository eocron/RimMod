using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SteamWorkshopSynchronizer.Core
{
    public interface ITableEntityProvider<TEntity>
        where TEntity : ITableEntity
    {
        Task<List<TEntity>> GetAllEntitiesAsync(CancellationToken ct);

        Task<TEntity> GetEntityAsync(string key, CancellationToken ct);
    }
}