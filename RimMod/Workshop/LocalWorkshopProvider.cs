using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RimMod.Helpers;
using RimMod.Workshop.Entities;

namespace RimMod.Workshop
{
    public sealed class LocalWorkshopProvider : IWorkshopProvider
    {
        private readonly string _folder;

        public LocalWorkshopProvider(string folder)
        {
            _folder = folder;
        }

        public async Task<WorkshopItemDetails> GetItemAsync(long fileId, CancellationToken cancellationToken)
        {
            var path = PathHelper.GetLocalDetailsPath(_folder, fileId);
            if (!File.Exists(path))
                return null;
            var json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<WorkshopItemDetails>(json);
        }

        public async Task<IList<WorkshopItemDetails>> GetItemsAsync(ICollection<long> itemIds, CancellationToken cancellationToken)
        {
            return await Task.WhenAll(itemIds.Select(x => GetItemAsync(x, cancellationToken))).ConfigureAwait(false);
        }

        public async Task<IList<WorkshopItemDetails>> GetAllItemsAsync(CancellationToken cancellationToken)
        {
            if (!Directory.Exists(_folder))
                return Array.Empty<WorkshopItemDetails>();

            var all = await Task.WhenAll(
                Directory
                    .GetDirectories(_folder, "*", SearchOption.TopDirectoryOnly)
                    .Select(TryGetItemId)
                    .Where(x => x > 0)
                    .Select(x => GetItemAsync(x, cancellationToken)));

            return all.Where(x => x != null).ToList();
        }

        private long TryGetItemId(string folderPath)
        {
            try
            {
                return long.Parse(new DirectoryInfo(folderPath).Name);
            }
            catch
            {
                return -1;
            }
        }
    }
}