using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SteamWorkshopSynchronizer.Steam.Contract;

namespace SteamWorkshopSynchronizer.Steam
{
    public class SteamWorkshopTableEntityProvider : ITableEntityProvider<SteamWorkshopTableEntity>
    {        
        private const string SteamGetPublishedFileDetailsUrl = "ISteamRemoteStorage/GetPublishedFileDetails/v1/";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _clientName;
        private readonly long[] _allFileIds;

        public SteamWorkshopTableEntityProvider(IHttpClientFactory httpClientFactory, string clientName, long[] allFileIds)
        {
            _httpClientFactory = httpClientFactory;
            _clientName = clientName;
            _allFileIds = allFileIds;
        }

        public async Task<List<SteamWorkshopTableEntity>> GetAllEntitiesAsync(CancellationToken ct)
        {
            return await GetFilesAsync(_allFileIds, ct).ConfigureAwait(false);
        }

        public async Task<SteamWorkshopTableEntity> GetEntityAsync(string key, CancellationToken ct)
        {
            var all = await GetFilesAsync(new[] { long.Parse(key) }, ct).ConfigureAwait(false);
            return all.Single();
        }

        private async Task<List<SteamWorkshopTableEntity>> GetFilesAsync(IEnumerable<long> fileIds, CancellationToken ct)
        {
            var content = new Dictionary<string, string>();
            int count = 0;
            foreach (var fileId in fileIds)
            {
                content[$"publishedfileids[{count++}]"] = fileId.ToString();
            }
            content["itemcount"] = count.ToString();
            using var form = new FormUrlEncodedContent(content);
            using var client = _httpClientFactory.CreateClient(_clientName);
            using var response = await client.PostAsync(SteamGetPublishedFileDetailsUrl, form, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            var apiResponse = JsonConvert.DeserializeObject<SteamApiResponse>(json);
            return apiResponse.Response.Details.Select(Convert).ToList();
        }

        private static SteamWorkshopTableEntity Convert(WorkshopItemDetails details)
        {
            var lastUpdated = DateTimeOffset
                .FromUnixTimeSeconds(details.LastUpdatedTimestamp)
                .ToUniversalTime()
                .DateTime;
            return new SteamWorkshopTableEntity
            {
                Key = details.ItemId.ToString(),
                FileId = details.ItemId,
                AppId = details.AppId,
                LastUpdated = lastUpdated,
                ETag = lastUpdated.Ticks.ToString(),
                EscapedTitle = details.EscapedTitle
            };
        }
    }
}