using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RimMod.Settings;
using RimMod.Workshop.Entities;

namespace RimMod.Workshop
{
    public sealed class SteamWorkshopProvider : IWorkshopProvider
    {
        private const string SteamGetPublishedFileDetailsUrl = "ISteamRemoteStorage/GetPublishedFileDetails/v1/";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IList<long> _allItemIds;

        public SteamWorkshopProvider(IHttpClientFactory httpClientFactory, IList<long> allItemIds)
        {
            _httpClientFactory = httpClientFactory;
            _allItemIds = allItemIds;
        }

        public async Task<WorkshopItemDetails> GetItemAsync(long fileId, CancellationToken cancellationToken)
        {
            using var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "itemcount", "1" },
                { "publishedfileids[0]", fileId.ToString() }
            });
            using var client = _httpClientFactory.CreateClient(ApplicationConst.SteamHttpClientName);
            using var response = await client.PostAsync(SteamGetPublishedFileDetailsUrl, form, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var obj = JObject.Parse(json).SelectToken("$.response.publishedfiledetails[0]").ToObject<WorkshopItemDetails>();
            return obj;
        }

        public async Task<IList<WorkshopItemDetails>> GetItemsAsync(ICollection<long> itemIds, CancellationToken cancellationToken)
        {
            return await Task.WhenAll(itemIds.Select(x => GetItemAsync(x, cancellationToken))).ConfigureAwait(false);
        }

        public Task<IList<WorkshopItemDetails>> GetAllItemsAsync(CancellationToken cancellationToken)
        {
            return GetItemsAsync(_allItemIds, cancellationToken);
        }
    }
}
