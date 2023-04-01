using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SteamWorkshopSynchronizer.Commands;

namespace SteamWorkshopSynchronizer.Steam
{
    public class WindowsSteamCmdDownloadJob : IAsyncJob
    {
        private readonly Uri _steamCmdDownloadLink;
        private readonly string _steamCmdTargetFolder;
        private readonly string _tmpFolder;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _httpClientName;
        private readonly ILogger _logger;
        private readonly bool _forceUpdate;

        public WindowsSteamCmdDownloadJob(IHttpClientFactory httpClientFactory, string httpClientName, ILogger logger, Uri steamCmdDownloadLink, string steamCmdTargetFolder, string tmpFolder, bool forceUpdate)
        {
            _steamCmdDownloadLink = steamCmdDownloadLink;
            _steamCmdTargetFolder = steamCmdTargetFolder;
            _tmpFolder = tmpFolder;
            _httpClientFactory = httpClientFactory;
            _httpClientName = httpClientName;
            _logger = logger;
            _forceUpdate = forceUpdate;
        }

        public async Task RunAsync(CancellationToken ct)
        {
            if (!_forceUpdate && Directory.Exists(_steamCmdTargetFolder))
            {
                return;
            }
            await UpdateSteamCmdAsync(_steamCmdTargetFolder, ct).ConfigureAwait(false);
        }

        private async Task UpdateSteamCmdAsync(string targetFolder, CancellationToken ct)
        {
            var tmpFolder = Path.Combine(_tmpFolder, "steamcmd_tmp_download", DateTime.UtcNow.ToFileTime().ToString());
            Directory.CreateDirectory(tmpFolder);
            try
            {
                var tmpZipFilePath = Path.Combine(tmpFolder, "tmp.zip");
                _logger.LogInformation("Downloading SteamCMD...");
                await DownloadFileAsync(_steamCmdDownloadLink, tmpZipFilePath, ct);

                var tmpExtractedFolder = Path.Combine(tmpFolder, "extracted");
                Directory.CreateDirectory(tmpExtractedFolder);
                _logger.LogInformation("Extracting SteamCMD...");
                ExtractFiles(tmpZipFilePath, tmpExtractedFolder);

                DeleteDirectoryIfExists(targetFolder);
                Directory.Move(tmpExtractedFolder, targetFolder);
                _logger.LogInformation("SteamCMD downloaded.");
            }
            finally
            {
                DeleteDirectoryIfExists(tmpFolder);
            }
        }
        
        private static void DeleteDirectoryIfExists(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath, true);
            }
        }

        private void ExtractFiles(string sourceFilePath, string targetFolder)
        {
            ZipFile.ExtractToDirectory(sourceFilePath, targetFolder);
        }
        private async Task DownloadFileAsync(Uri uri, string filePath, CancellationToken ct)
        {
            using var client = _httpClientFactory.CreateClient(_httpClientName);
            await using var stream = await client.GetStreamAsync(_steamCmdDownloadLink, ct).ConfigureAwait(false);
            await using var fileStream = File.OpenWrite(filePath);
            await stream.CopyToAsync(fileStream, ct).ConfigureAwait(false);
        }
    }
}