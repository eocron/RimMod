using System.Threading;
using System.Threading.Tasks;
using RimMod.Workshop.Entities;

namespace RimMod.Workshop
{
    public interface IWorkshopSynchronizationManager
    {
        Task UpdateAsync(WorkshopItemDetails oldItem, WorkshopItemDetails newItem, CancellationToken cancellationToken);

        Task CreateAsync(WorkshopItemDetails newItem, CancellationToken cancellationToken);

        Task DeleteAsync(long itemId, CancellationToken cancellationToken);
    }
}