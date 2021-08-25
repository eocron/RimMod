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
    internal sealed class SteamModDownloader : ISteamModDownloader
    {
        private readonly ModDownloadSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SteamModDownloader> _logger;
        private readonly IGameModDirectoryDetector _detector;
        private readonly string RequestUrl = "/api/download/request";
        private readonly string StatusUrl = "/api/download/status";
        private readonly string TransmitUrl = "/api/download/transmit?uuid=";
        private readonly TimeSpan _jobTimeout = TimeSpan.FromSeconds(10);
        private readonly TimeSpan _jobStatusCheckInterval = TimeSpan.FromSeconds(1);
        private readonly string _baseAddress;
        public SteamModDownloader(ModDownloadSettings settings, IHttpClientFactory httpClientFactory, ILogger<SteamModDownloader> logger, IGameModDirectoryDetector detector)
        {
            _settings = settings;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _detector = detector;
            _baseAddress = _settings.SteamWorkshopLink ?? throw new ArgumentNullException(nameof(_settings.SteamWorkshopLink));
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var outputFolder = _settings.OutputFolder ?? (await _detector.Detect(cancellationToken)) ?? throw new ArgumentNullException(nameof(_settings.OutputFolder));
            using var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_baseAddress);
            var exceptions = new List<string>();
            foreach (var link in _settings.Links)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var uri = new Uri(link);
                var parameters = HttpUtility.ParseQueryString(uri.Query);
                var fileId = long.Parse(parameters["id"]);

                var downloadLink = await GetDownloadLink(client, fileId, cancellationToken).ConfigureAwait(false);
                var modFolderPath = await DownloadMod(client, downloadLink, outputFolder, cancellationToken).ConfigureAwait(false);
                exceptions.Add(modFolderPath);
            }

            if(_settings.CleanOutOtherMods)
                DeleteAllExcept(outputFolder, exceptions);
        }

        private void DeleteAllExcept(string folderPath, List<string> exceptions)
        {
            var toDelete = Directory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly).Except(exceptions);
            foreach (var d in toDelete)
                Directory.Delete(d);
        }

        private async Task<string> DownloadMod(HttpClient client, string uri, string folderPath, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(folderPath);

            using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var fileName = response.Content.Headers.ContentDisposition.FileName;
            var tmpFolder = Path.Combine(folderPath, fileName + "_tmp");

            var tmpFileName = Path.Combine(tmpFolder, fileName);
            var tmpExtractedFolderPath = Path.Combine(tmpFileName + "_extracted");
            var finalFolderPath = Path.Combine(folderPath, Path.GetFileNameWithoutExtension(fileName));
            try
            {
                Directory.CreateDirectory(tmpFolder);
                Directory.CreateDirectory(tmpExtractedFolderPath);
                _logger.LogInformation($"Downloading {uri}...");
                using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
                using (var tmpStream = File.OpenWrite(tmpFileName))
                {
                    await stream.CopyToAsync(tmpStream, cancellationToken).ConfigureAwait(false);
                }
                _logger.LogInformation($"Extracting {uri}...");
                ZipFile.ExtractToDirectory(tmpFileName, tmpExtractedFolderPath, true);

                _logger.LogInformation($"Updating {uri}...");
                if (_settings.OverwriteFiles)
                    if (Directory.Exists(finalFolderPath))
                        Directory.Delete(finalFolderPath, true);
                Directory.Move(tmpExtractedFolderPath, finalFolderPath);
                _logger.LogInformation($"Done {uri}.");
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

            _logger.LogDebug($"Retrieved job id {uuid} for {fileId}");
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