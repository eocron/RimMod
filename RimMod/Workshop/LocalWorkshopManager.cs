using System;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RimMod.Helpers;
using RimMod.Workshop.Entities;

namespace RimMod.Workshop
{
    public sealed class LocalWorkshopManager : IWorkshopManager
    {
        private readonly string _folder;

        public LocalWorkshopManager(string folder)
        {
            _folder = folder;
        }

        public string GetFolder(long itemId)
        {
            return Path.Combine(_folder, itemId.ToString());
        }

        public async Task SaveAsync(WorkshopItemDetails details, CancellationToken cancellationToken)
        {
            if (details == null)
                throw new ArgumentNullException(nameof(details));

            var path = PathHelper.GetLocalDetailsPath(_folder, details.ItemId);
            await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(details, Formatting.Indented), cancellationToken).ConfigureAwait(false);
        }

        public Task DeleteAsync(long itemId, CancellationToken cancellationToken)
        {
            var path = Path.Combine(_folder, itemId.ToString());
            if(Directory.Exists(path))
                Directory.Delete(path, true);
            return Task.CompletedTask;
        }
    }
}