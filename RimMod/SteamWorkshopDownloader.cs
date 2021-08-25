using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace RimMod
{
    internal sealed class SteamWorkshopDownloader : ISteamModDownloader
    {
        private readonly ModDownloadSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SteamWorkshopDownloader> _logger;
        private readonly string RequestUrl = "/api/download/request";
        private readonly string StatusUrl = "/api/download/status";
        private readonly string TransmitUrl = "/api/download/transmit?uuid=";
        private readonly TimeSpan _jobTimeout = TimeSpan.FromSeconds(10);
        private readonly TimeSpan _jobStatusCheckInterval = TimeSpan.FromSeconds(1);
        private readonly string _baseAddress;
        public SteamWorkshopDownloader(ModDownloadSettings settings, IHttpClientFactory httpClientFactory, ILogger<SteamWorkshopDownloader> logger)
        {
            _settings = settings;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _baseAddress = _settings.SteamWorkshopLink ?? throw new ArgumentNullException(nameof(_settings.SteamWorkshopLink));
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var outputFolder = _settings.ModFolder ?? throw new ArgumentNullException(nameof(_settings.ModFolder));
            var modFileIds = GetModLinks(_settings.ModLinks ?? throw new ArgumentNullException(nameof(_settings.ModLinks)))
                .Select(x=> {
                    var uri = new Uri(x);
                    var parameters = HttpUtility.ParseQueryString(uri.Query);
                    var fileId = long.Parse(parameters["id"]);
                    return fileId;
                })
                .Where(x=> x > 0)
                .Distinct()
                .ToList();

            Directory.CreateDirectory(outputFolder);

            var fjoin = Directory.GetDirectories(outputFolder, "*", SearchOption.TopDirectoryOnly)
                .FullOuterJoin(modFileIds, x => Path.GetFileName(x), x => x.ToString(), (o, n, k) => new { o, n, k })
                .ToList();

            using var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_baseAddress);
            foreach (var obj in fjoin)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (obj.o != null && obj.n != 0 && _settings.Mode.HasFlag(UpdateMode.Update))//update
                {
                    var downloadLink = await GetDownloadLink(client, obj.n, cancellationToken).ConfigureAwait(false);
                    await DownloadMod(client, downloadLink, outputFolder, obj.n.ToString(), cancellationToken).ConfigureAwait(false);
                    _logger.LogInformation($"Updated mod {obj.n}");
                }
                else if(obj.o == null && obj.n != 0 && _settings.Mode.HasFlag(UpdateMode.Create))//create
                {
                    var downloadLink = await GetDownloadLink(client, obj.n, cancellationToken).ConfigureAwait(false);
                    await DownloadMod(client, downloadLink, outputFolder, obj.n.ToString(), cancellationToken).ConfigureAwait(false);
                    _logger.LogInformation($"Added mod {obj.n}");
                }
                else if(obj.o != null && obj.n == 0 && _settings.Mode.HasFlag(UpdateMode.Delete))//delete
                {
                    Directory.Delete(obj.o, true);
                    _logger.LogInformation($"Deleted mod {obj.k}");
                }
            }

        }

        private List<string> GetModLinks(string modLinksFilePath)
        {
            return File.ReadAllLines(modLinksFilePath)
                .Select(x => TrimCommentsAndWhitespaces(x))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        private string TrimCommentsAndWhitespaces(string input)
        {
            var commentIdx = input.IndexOf("#");
            if (commentIdx >= 0)
                input = input.Substring(0, commentIdx);
            return input.Trim();
        }

        private async Task<string> DownloadMod(HttpClient client, string uri, string folderPath, string modName, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(folderPath);

            using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var tmpFolder = Path.Combine(folderPath, modName + "_tmp");

            var tmpFileName = Path.Combine(tmpFolder, modName);
            var tmpExtractedFolderPath = Path.Combine(tmpFileName + "_extracted");
            var finalFolderPath = Path.Combine(folderPath, modName);

            if (Directory.Exists(tmpFolder))
                Directory.Delete(tmpFolder, true);
            try
            {
                Directory.CreateDirectory(tmpFolder);
                Directory.CreateDirectory(tmpExtractedFolderPath);
                _logger.LogInformation($"Downloading {modName}...");
                using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
                using (var tmpStream = File.OpenWrite(tmpFileName))
                {
                    await stream.CopyToAsync(tmpStream, cancellationToken).ConfigureAwait(false);
                }
                _logger.LogInformation($"Extracting {modName}...");
                ZipFile.ExtractToDirectory(tmpFileName, tmpExtractedFolderPath, true);

                if (Directory.Exists(finalFolderPath))
                    Directory.Delete(finalFolderPath, true);
                Directory.Move(tmpExtractedFolderPath, finalFolderPath);
                _logger.LogInformation($"Done {modName}.");
                return finalFolderPath;
            }
            finally
            {
                if (Directory.Exists(tmpFolder))
                    Directory.Delete(tmpFolder, true);
            }
        }

        private async Task<string> GetDownloadLink(HttpClient client, long fileId, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Preparing {fileId}...");
            var uuid = await InitializeDownloadJob(client, fileId, cancellationToken).ConfigureAwait(false);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_jobTimeout);
            while (true)
            {
                cts.Token.ThrowIfCancellationRequested();
                var status = await GetDownloadJobStatus(client, uuid, cts.Token).ConfigureAwait(false);
                if (status == "prepared")
                    break;

                await Task.Delay(_jobStatusCheckInterval, cts.Token).ConfigureAwait(false);
            }

            return TransmitUrl + uuid;
        }
        private async Task<string> GetDownloadJobStatus(HttpClient client, string uuid, CancellationToken cancellationToken)
        {
            var request = new
            {
                uuids = new[] { uuid }
            };
            using var response = await client
                .PostAsync(StatusUrl, new StringContent(JObject.FromObject(request).ToString(), Encoding.UTF8), cancellationToken)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var obj = JObject.Parse(responseJson);
            var statusObj = obj[uuid];
            var statusStr = statusObj["status"].ToString();
            return statusStr;
        }
        private async Task<string> InitializeDownloadJob(HttpClient client, long fileId, CancellationToken cancellationToken)
        {
            var request = new
            {
                publishedFileId = fileId,
                collectionId = (object)null,
                extract = true,
                hidden = true,
                direct = false
            };
            using var response = await client
                .PostAsync(RequestUrl, new StringContent(JObject.FromObject(request).ToString(), Encoding.UTF8), cancellationToken)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var responseObj = JObject.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            return responseObj["uuid"].ToString();
        }
    }
}