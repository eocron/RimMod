using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RimMod.Github.Entities;
using RimMod.Steam;

namespace RimMod.Github
{
    public sealed class GithubDownloader : BaseDownloader<GithubItemId, GithubItem>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _httpClientName;

        public GithubDownloader(IHttpClientFactory httpClientFactory, string httpClientName)
        {
            _httpClientFactory = httpClientFactory;
            _httpClientName = httpClientName;
        }

        protected override Task OnDownloadIntoFolderAsync(string folder, GithubItem item, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
