using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RimMod.Helpers;

namespace RimMod.Synchronization
{
    public sealed class LocalItemProvider : IItemProvider<IItemId, IItem>
    {
        private readonly string _folder;
        private readonly ILogger _logger;

        public LocalItemProvider(string folder, ILogger logger)
        {
            _folder = folder;
            _logger = logger;
        }

        public async Task<IItem> GetItemAsync(IItemId itemId, CancellationToken cancellationToken)
        {
            var path = LocalItemHelper.GetLocalDetailsPath(_folder, itemId);
            if (!File.Exists(path))
                return null;
            var text = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
            return LocalItemHelper.Deserialize<IItem>(text);
        }

        public async Task<IList<IItem>> GetItemsAsync(ICollection<IItemId> itemIds, CancellationToken cancellationToken)
        {
            return await Task.WhenAll(itemIds.Select(x => GetItemAsync(x, cancellationToken))).ConfigureAwait(false);
        }

        public async Task<IList<IItem>> GetAllItemsAsync(CancellationToken cancellationToken)
        {
            if (!Directory.Exists(_folder))
                return Array.Empty<IItem>();

            var all =
                await Task.WhenAll(
                Directory
                    .GetDirectories(_folder, "*", SearchOption.TopDirectoryOnly)
                    .Select(x=> TryGetItem(x, cancellationToken))).ConfigureAwait(false);

            return all.Where(x => x != null).ToList();
        }

        private async Task<IItem> TryGetItem(string folderPath, CancellationToken cancellationToken)
        {
            var filePath = LocalItemHelper.GetLocalDetailsPath(folderPath);
            if (!File.Exists(filePath))
                return null;

            var text = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            try
            {
                var item = LocalItemHelper.Deserialize<IItem>(text);
                return item;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to deserialize {itemPath}", filePath);
                return null;
            }
        }
    }
}