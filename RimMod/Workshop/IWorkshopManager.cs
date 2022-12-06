using System.Threading;
using System.Threading.Tasks;
using RimMod.Workshop.Entities;

namespace RimMod.Workshop
{
    public interface IWorkshopManager
    {
        string GetFolder(long itemId);
        Task AddDetailsAsync(WorkshopItemDetails details, CancellationToken cancellationToken);
        Task DeleteAsync(long itemId, CancellationToken cancellationToken);
    }
}