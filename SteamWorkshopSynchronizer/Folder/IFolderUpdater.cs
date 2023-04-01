using System.Threading;
using System.Threading.Tasks;

namespace SteamWorkshopSynchronizer.Folder
{
    public interface IFolderUpdater<in T>
    {
        Task UpdateAsync(T entity, string folderPath, CancellationToken ct);
    }
}