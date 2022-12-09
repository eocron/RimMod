using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RimMod.OnlineDownloaders;
using RimMod.Synchronization;

namespace RimMod.Steam;

public abstract class BaseHttpDownloader<TId, TItem> : IItemDownloader
where TItem : IItem
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _httpClientName;
    private readonly ILogger _logger;
    private readonly int _unwrap;
    public IReadOnlySet<Type> SupportedItemTypes { get; }

    protected BaseHttpDownloader(IHttpClientFactory httpClientFactory, string httpClientName, ILogger logger, int unwrap)
    {
        _httpClientFactory = httpClientFactory;
        _httpClientName = httpClientName;
        _logger = logger;
        _unwrap = unwrap;
        SupportedItemTypes = new HashSet<Type>(new[] { typeof(TItem) });
    }

    public Task DownloadIntoFolderAsync(string folder, IItem item, CancellationToken cancellationToken)
    {
        if (folder == null)
            throw new ArgumentNullException(nameof(folder));
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        if (!SupportedItemTypes.Contains(item.GetType()))
            throw new NotSupportedException(item.GetType().Name);

        return OnDownloadIntoFolderAsync(folder, (TItem)item, cancellationToken);
    }

    protected abstract Task<string> GetDownloadLink(TItem item, CancellationToken cancellationToken);

    private async Task OnDownloadIntoFolderAsync(string folder, TItem item, CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope("Download {itemId}:{itemName} into {folder}", item.Id, item.Name, folder);

        using var client = _httpClientFactory.CreateClient(_httpClientName);
        _logger.Log(LogLevel.Debug, "Preparing {itemName}...", item.Name);
        var downloadLink = await GetDownloadLink(item, cancellationToken).ConfigureAwait(false);

        using var downloadResponse = await client.GetAsync(downloadLink, cancellationToken).ConfigureAwait(false);
        var tmpFolder = folder + "_tmp";
        if (Directory.Exists(tmpFolder))
            Directory.Delete(tmpFolder, true);
        Directory.CreateDirectory(tmpFolder);
        try
        {
            var zipFilePath = Path.Combine(tmpFolder, "downloaded.zip");
            var unzipFolderPath = Path.Combine(tmpFolder, "unzipped");
            _logger.Log(LogLevel.Debug, "Downloading {itemName}...", item.Name);
            await using var fo = File.OpenWrite(zipFilePath);
            await using var fi = await downloadResponse.Content.ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);
            await fi.CopyToAsync(fo, cancellationToken).ConfigureAwait(false);
            fo.Close();
            fi.Close();

            _logger.Log(LogLevel.Debug, "Extracting {itemName}...", item.Name);
            ZipFile.ExtractToDirectory(zipFilePath, unzipFolderPath, false);
            if (_unwrap > 0)
            {
                for (int i = 0; i < _unwrap; i++)
                {
                    unzipFolderPath = Directory.GetDirectories(unzipFolderPath).Single();
                }
                _logger.Log(LogLevel.Debug, "Unwrapped path to {unwrappedPath}", unzipFolderPath);
            }

            _logger.Log(LogLevel.Debug, "Updating {itemName}...", item.Name);
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);

            Directory.Move(unzipFolderPath, folder);
            _logger.LogDebug("Done {itemName}.", item.Name);
        }
        finally
        {
            if (Directory.Exists(tmpFolder))
                Directory.Delete(tmpFolder, true);
        }
    }
}