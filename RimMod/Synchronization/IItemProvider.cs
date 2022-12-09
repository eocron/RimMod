using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RimMod.Synchronization
{
    public interface IItemProvider<TId, TItem>
    {
        Task<IList<TItem>> GetItemsAsync(IList<TId> itemIds, CancellationToken cancellationToken);

        Task<IList<TItem>> GetAllItemsAsync(CancellationToken cancellationToken);
    }
}