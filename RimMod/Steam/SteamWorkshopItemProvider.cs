using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RimMod.Steam.Entities;
using RimMod.Synchronization;

namespace RimMod.Steam
{
    public sealed class SteamWorkshopItemProvider : IItemProvider<SteamWorkshopItemId, SteamWorkshopItem>
    {
        private const string SteamGetPublishedFileDetailsUrl = "ISteamRemoteStorage/GetPublishedFileDetails/v1/";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _httpClientName;
        private readonly IList<SteamWorkshopItemId> _allItemIds;

        public SteamWorkshopItemProvider(IHttpClientFactory httpClientFactory, string httpClientName, IList<SteamWorkshopItemId> allItemIds)
        {
            _httpClientFactory = httpClientFactory;
            _httpClientName = httpClientName;
            _allItemIds = allItemIds;
        }

        public async Task<SteamWorkshopItem> GetItemAsync(SteamWorkshopItemId itemId, CancellationToken cancellationToken)
        {
            using var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "itemcount", "1" },
                { "publishedfileids[0]", itemId.FileId.ToString() }
            });
            using var client = _httpClientFactory.CreateClient(_httpClientName);
            using var response = await client.PostAsync(SteamGetPublishedFileDetailsUrl, form, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var apiResponse = JsonConvert.DeserializeObject<SteamApiResponse>(json);
            var obj = apiResponse.Response.Details.Single();
            return new SteamWorkshopItem(itemId, obj.EscapedTitle, obj.LastUpdatedTimestamp.ToString(), obj);
        }

        public async Task<IList<SteamWorkshopItem>> GetItemsAsync(IList<SteamWorkshopItemId> itemIds,
            CancellationToken cancellationToken)
        {
            return await Task.WhenAll(itemIds.Select(x => GetItemAsync(x, cancellationToken))).ConfigureAwait(false);
        }

        public Task<IList<SteamWorkshopItem>> GetAllItemsAsync(CancellationToken cancellationToken)
        {
            return GetItemsAsync(_allItemIds, cancellationToken);
        }
    }
}
