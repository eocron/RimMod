using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octokit;
using RimMod.Github.Entities;
using RimMod.Synchronization;

namespace RimMod.Github
{
    public sealed class GithubItemProvider : IItemProvider<GithubItemId, GithubItem>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _httpClientName;
        private readonly IList<GithubItemId> _allItemIds;

        public GithubItemProvider(IHttpClientFactory httpClientFactory, string httpClientName, IList<GithubItemId> allItemIds)
        {
            _httpClientFactory = httpClientFactory;
            _httpClientName = httpClientName;
            _allItemIds = allItemIds;
        }

        public async Task<IList<GithubItem>> GetItemsAsync(IList<GithubItemId> itemIds, CancellationToken cancellationToken)
        {
            if (!itemIds.Any())
                return Array.Empty<GithubItem>();
            
            using var client = _httpClientFactory.CreateClient(_httpClientName);
            var result = await Task.WhenAll(itemIds.Select(x=> GetGithubItemAsync(client, x, cancellationToken))).ConfigureAwait(false);
            return result;
        }

        public Task<IList<GithubItem>> GetAllItemsAsync(CancellationToken cancellationToken)
        {
            return GetItemsAsync(_allItemIds, cancellationToken);
        }

        private async Task<GithubItem> GetGithubItemAsync(HttpClient client, GithubItemId id, CancellationToken cancellationToken)
        {
            var link = $"repos/{id.Username}/{id.RepositoryName}/releases/latest";
            using var response = await client.GetAsync(link, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var obj = JsonConvert.DeserializeObject<GithubReleaseResponse>(json);
            var asset = obj.Assets.Single();
            return new GithubItem(
                new GithubItemId(null, null, null, obj.Id), 
                Path.GetFileNameWithoutExtension(asset.Name), 
                obj.PublishedAt.Ticks.ToString(), 
                asset.DownloadLink);
        }
    }
}
