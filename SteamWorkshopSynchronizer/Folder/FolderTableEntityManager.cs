﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SteamWorkshopSynchronizer.Folder
{
    public class FolderTableEntityManager<T> : ITableEntityProvider<T>, ITableEntityManager<T>
        where T : class, IFileTableEntity
    {
        private readonly string _folder;
        private readonly ILogger _logger;

        public FolderTableEntityManager(string folder, ILogger logger)
        {
            _folder = folder;
            _logger = logger;
        }

        public async Task<List<T>> GetAllEntitiesAsync(CancellationToken ct)
        {
            if (!Directory.Exists(_folder))
            {
                return new List<T>();
            }

            var dict = await InternalGetAllAsync(ct).ConfigureAwait(false);
            return dict.Values.Select(x => x.Manifest).ToList();
        }

        public async Task<T> GetEntityAsync(string key, CancellationToken ct)
        {
            var dict = await InternalGetAllAsync(ct).ConfigureAwait(false);
            return dict[key].Manifest;
        }

        public async Task DeleteEntityAsync(string key, CancellationToken ct)
        {
            var found = (await InternalGetOrDefaultAsync(key, ct).ConfigureAwait(false))?.DirectoryPath;
            if(found == null || !Directory.Exists(found))
                return;
            Directory.Delete(found, true);
        }

        public async Task UpdateEntityAsync(T entity, CancellationToken ct)
        {
            await CreateOrUpdateManifestAsync(entity, ct).ConfigureAwait(false);
        }

        public async Task CreateEntityAsync(T entity, CancellationToken ct)
        {
            await CreateOrUpdateManifestAsync(entity, ct).ConfigureAwait(false);
        }

        private async Task CreateOrUpdateManifestAsync(T entity, CancellationToken ct)
        {
            var found = (await InternalGetOrDefaultAsync(entity.Key, ct).ConfigureAwait(false))?.DirectoryPath ?? Path.Combine(_folder, entity.EscapedTitle);
            var manifestPath = GetManifestPath(found);
            if(!Directory.Exists(found))
                Directory.CreateDirectory(found);
            await File.WriteAllTextAsync(manifestPath, JsonConvert.SerializeObject(entity), ct).ConfigureAwait(false);
        }
        
        
        private async Task<IDictionary<string, Entry>> InternalGetAllAsync(CancellationToken ct)
        {
            if (!Directory.Exists(_folder))
            {
                return new Dictionary<string, Entry>();
            }
            var all = await Task
                .WhenAll(Directory.GetDirectories(_folder, "*", SearchOption.TopDirectoryOnly)
                    .Select(x => TryConvertAsync(x, ct)).ToList()).ConfigureAwait(false);
            return all.Where(x => x != null).ToDictionary(x=> x.Manifest.Key);
        }

        private async Task<Entry> InternalGetOrDefaultAsync(string key, CancellationToken ct)
        {
            var dict = await InternalGetAllAsync(ct).ConfigureAwait(false);
            if (dict.TryGetValue(key, out var tmp))
                return tmp;
            return null;
        }

        private async Task<Entry> TryConvertAsync(string dirPath, CancellationToken ct)
        {
            var manifestPath = GetManifestPath(dirPath);
            if (!File.Exists(manifestPath))
            {
                return null;
            }

            var text = await File.ReadAllTextAsync(manifestPath, ct).ConfigureAwait(false);
            try
            {
                return new Entry
                {
                    DirectoryPath = dirPath,
                    Manifest = JsonConvert.DeserializeObject<T>(text)
                };
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to deserialize file manifest: {manifestPath}", manifestPath);
            }

            return null;
        }

        private static string GetManifestPath(string dirPath)
        {
            return Path.Combine(dirPath, "sws_info.json");
        }
        
        private class Entry
        {
            public string DirectoryPath { get; set; }
            
            public T Manifest { get; set; }
        }
    }
}