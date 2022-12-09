using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RimMod.Helpers;

namespace RimMod.Synchronization
{
    public sealed class LocalItemManager : IItemManager
    {
        private readonly string _folder;
        public LocalItemManager(string folder)
        {
            _folder = folder;
        }

        public string GetFolder(string name)
        {
            var path = LocalItemHelper.GetLocalDetailsFolder(_folder, name);
            return path;
        }

        public async Task AddItemAsync(IItem item, CancellationToken cancellationToken)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var path = LocalItemHelper.GetLocalDetailsPath(_folder, item.Id);
            await File.WriteAllTextAsync(path, LocalItemHelper.Serialize(item), cancellationToken)
                .ConfigureAwait(false);
        }

        public Task DeleteAsync(IItemId itemId, CancellationToken cancellationToken)
        {
            var path = LocalItemHelper.GetLocalDetailsPath(_folder, itemId);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            return Task.CompletedTask;
        }
    }
}