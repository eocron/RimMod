using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SteamWorkshopSynchronizer.Folder;

namespace SteamWorkshopSynchronizer.Steam
{
    public sealed class SteamWorkshopTableEntityUpdater : IFolderUpdater<SteamWorkshopTableEntity>
    {
        private readonly ISteamClient _client;

        public SteamWorkshopTableEntityUpdater(ISteamClient client)
        {
            _client = client;
        }

        public async Task UpdateAsync(SteamWorkshopTableEntity entity, string folderPath, CancellationToken ct)
        {
            var pathToFiles = await _client.DownloadWorkshopItemAndReturnPathAsync(entity.AppId, entity.FileId, ct).ConfigureAwait(false);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
            Directory.Move(pathToFiles, folderPath);
        }
    }
}