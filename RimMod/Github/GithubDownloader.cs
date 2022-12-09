using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RimMod.Github.Entities;
using RimMod.Steam;

namespace RimMod.Github
{
    public sealed class GithubDownloader : BaseHttpDownloader<GithubItemId, GithubItem>
    {
        public GithubDownloader(IHttpClientFactory httpClientFactory, string httpClientName, ILogger logger) : base(httpClientFactory, httpClientName, logger, 1)
        {
        }

        protected override Task<string> GetDownloadLink(GithubItem item, CancellationToken cancellationToken)
        {
            return Task.FromResult(item.DownloadLink);
        }
    }
}
