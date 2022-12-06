//using Microsoft.Extensions.Logging;
//using RimMod.Workshop.Entities;
//using System.IO;
//using System.IO.Compression;
//using System.Net.Http;
//using System.Threading;
//using System.Threading.Tasks;

//namespace RimMod
//{
//    internal sealed class SteamWorkshopDownloader 
//    {
//        private readonly ILogger<SteamWorkshopDownloader> _logger;


//        private async Task<string> DownloadMod(HttpClient client, string uri, string folderPath, WorkshopItemDetails details, string finalFolderPath, CancellationToken cancellationToken)
//        {
//            Directory.CreateDirectory(folderPath);
//            var modName = Path.GetFileName(finalFolderPath);
//            using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
//            response.EnsureSuccessStatusCode();
//            var tmpFolder = Path.Combine(folderPath, modName + "_tmp");

//            var tmpFileName = Path.Combine(tmpFolder, modName);
//            var tmpExtractedFolderPath = Path.Combine(tmpFileName + "_extracted");

//            if (Directory.Exists(tmpFolder))
//                Directory.Delete(tmpFolder, true);
//            try
//            {
//                Directory.CreateDirectory(tmpFolder);
//                Directory.CreateDirectory(tmpExtractedFolderPath);
//                _logger.LogDebug($"Downloading {modName}...");
//                using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
//                using (var tmpStream = File.OpenWrite(tmpFileName))
//                {
//                    await stream.CopyToAsync(tmpStream, cancellationToken).ConfigureAwait(false);
//                }
//                _logger.LogDebug($"Extracting {modName}...");
//                ZipFile.ExtractToDirectory(tmpFileName, tmpExtractedFolderPath, true);
//                //await _detailsManager.SaveLocalDetails(tmpExtractedFolderPath, details, cancellationToken).ConfigureAwait(false);

//                //replace original
//                if (Directory.Exists(finalFolderPath))
//                    Directory.Delete(finalFolderPath, true);
//                Directory.Move(tmpExtractedFolderPath, finalFolderPath);
//                _logger.LogDebug($"Done {modName}.");
//                return finalFolderPath;
//            }
//            finally
//            {
//                if (Directory.Exists(tmpFolder))
//                    Directory.Delete(tmpFolder, true);
//            }
//        }
//    }
//}