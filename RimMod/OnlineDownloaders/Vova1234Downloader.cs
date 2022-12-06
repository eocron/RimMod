using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RimMod.Settings;
using RimMod.Workshop.Entities;

namespace RimMod.OnlineDownloaders
{
    public sealed class Vova1234Downloader : IWorkshopItemDownloader
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Vova1234Downloader> _logger;

        public Vova1234Downloader(IHttpClientFactory httpClientFactory, ILogger<Vova1234Downloader> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task DownloadIntoFolderAsync(string folder, WorkshopItemDetails details,
            CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope("Download {itemId}:{itemName} into {folder}", details.ItemId, details.EscapedTitle, folder);
            using var client = _httpClientFactory.CreateClient(ApplicationConst.Vova1234Downloader);
            _logger.Log(LogLevel.Information, "Preparing...");
            var downloadLink = await CreateDownloadLink(client, details, cancellationToken).ConfigureAwait(false);

            using var downloadResponse = await client.GetAsync(downloadLink, cancellationToken).ConfigureAwait(false);
            var tmpFolder = folder + "_tmp";
            if (Directory.Exists(tmpFolder))
                Directory.Delete(tmpFolder, true);
            Directory.CreateDirectory(tmpFolder);
            try
            {
                var zipFilePath = Path.Combine(tmpFolder, "downloaded.zip");
                var unzipFolderPath = Path.Combine(tmpFolder, "unzipped");
                _logger.Log(LogLevel.Information, "Downloading...");
                await using var fo = File.OpenWrite(zipFilePath);
                await using var fi = await downloadResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                await fi.CopyToAsync(fo, cancellationToken).ConfigureAwait(false);
                fo.Close();
                fi.Close();

                _logger.Log(LogLevel.Information, "Extracting...");
                ZipFile.ExtractToDirectory(zipFilePath, unzipFolderPath, false);
                _logger.Log(LogLevel.Information, "Updating...");
                if (Directory.Exists(folder))
                    Directory.Delete(folder, true);
                unzipFolderPath = Path.Combine(unzipFolderPath, details.ItemId.ToString());
                Directory.Move(unzipFolderPath, folder);
                _logger.LogDebug($"Done.");
            }
            finally
            {
                if (Directory.Exists(tmpFolder))
                    Directory.Delete(tmpFolder, true);
            }
        }

        private async Task<string> CreateDownloadLink(HttpClient client, WorkshopItemDetails details, CancellationToken cancellationToken)
        {
            var link = $"http://steamworkshop.download/online/steamonline.php";
            var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("item", details.ItemId.ToString()),
                new("app", details.AppId.ToString())
            });
            using var response = await client.PostAsync(link, content, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (html.Contains("Free space left"))
                throw new Exception("Unknown error: Free space left for " + details.EscapedTitle);

            var match = Regex.Match(html, "a href=[\"'](?<link>.+?)[\"']", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);
            if (match.Success)
                return match.Groups["link"].Value;

            throw new Exception("Invalid html? " + html);
        }
    }
}