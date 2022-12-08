using System.Threading;
using System.Threading.Tasks;

namespace RimMod.Synchronization
{
    public interface IItemSynchronizationManager
    {
        Task UpdateAsync(IItem oldItem, IItem newItem, CancellationToken cancellationToken);

        Task CreateAsync(IItem newItem, CancellationToken cancellationToken);

        Task DeleteAsync(IItemId itemId, CancellationToken cancellationToken);
    }
}