using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RimMod.Workshop.Entities;

namespace RimMod.Workshop
{
    public interface IWorkshopProvider
    {
        Task<IList<WorkshopItemDetails>> GetItemsAsync(ICollection<long> itemIds, CancellationToken cancellationToken);

        Task<IList<WorkshopItemDetails>> GetAllItemsAsync(CancellationToken cancellationToken);
    }
}