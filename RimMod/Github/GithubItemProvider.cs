using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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

        public Task<IList<GithubItem>> GetItemsAsync(ICollection<GithubItemId> itemIds, CancellationToken cancellationToken)
        {
            return Task.FromResult((IList<GithubItem>)new List<GithubItem>());
            throw new NotImplementedException();
        }

        public Task<IList<GithubItem>> GetAllItemsAsync(CancellationToken cancellationToken)
        {
            return GetItemsAsync(_allItemIds, cancellationToken);
        }
    }
}
