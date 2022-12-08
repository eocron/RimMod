using System.Threading;
using System.Threading.Tasks;

namespace RimMod.Synchronization
{
    public interface IItemManager
    {
        string GetFolder(IItemId itemId);
        Task AddItemAsync(IItem item, CancellationToken cancellationToken);
        Task DeleteAsync(IItemId itemId, CancellationToken cancellationToken);
    }
}