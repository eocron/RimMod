using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RimMod.Steam.Entities;

namespace RimMod.Steam
{
    public sealed class Vova1234SteamWorkshopDownloader : BaseHttpDownloader<SteamWorkshopItemId, SteamWorkshopItem>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _httpClientName;
        private readonly ILogger _logger;

        public Vova1234SteamWorkshopDownloader(IHttpClientFactory httpClientFactory, string httpClientName, ILogger logger) : base(httpClientFactory, httpClientName, logger, true)
        {
            _httpClientFactory = httpClientFactory;
            _httpClientName = httpClientName;
            _logger = logger;
        }

        protected override async Task<string> GetDownloadLink(SteamWorkshopItem item, CancellationToken cancellationToken)
        {
            var details = item.Details;
            var link = $"http://steamworkshop.download/online/steamonline.php";
            var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("item", details.ItemId.ToString()),
                new("app", details.AppId.ToString())
            });
            using var client = _httpClientFactory.CreateClient(_httpClientName);
            using var response = await client.PostAsync(link, content, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (html.Contains("Free space left"))
                throw new Exception("Free space left?");

            var match = Regex.Match(html, "a href=[\"'](?<link>.+?)[\"']",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);
            if (match.Success)
                return match.Groups["link"].Value;

            throw new Exception("Invalid html? " + html);
        }
    }
}