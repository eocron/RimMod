using System.Threading;
using System.Threading.Tasks;

namespace SteamWorkshopSynchronizer
{
    public interface ITableEntityManager<in TEntity>
        where TEntity : ITableEntity
    {
        Task DeleteEntityAsync(string key, CancellationToken ct);

        Task UpdateEntityAsync(TEntity entity, CancellationToken ct);

        Task CreateEntityAsync(TEntity entity, CancellationToken ct);
    }
}