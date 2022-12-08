using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RimMod.OnlineDownloaders;

namespace RimMod.Synchronization;

public sealed class ComplexItemDownloader : IItemDownloader
{
    private readonly Dictionary<Type, IItemDownloader[]> _all;
    private readonly HashSet<Type> _supported;
    public ComplexItemDownloader(IItemDownloader[] downloaders)
    {
        _all = downloaders
            .SelectMany(x =>
            {
                return x.SupportedItemTypes
                    .Select(y => new
                    {
                        type = y,
                        downloader = x
                    });
            })
            .GroupBy(x => x.type)
            .ToDictionary(
                x => x.Key,
                x => x
                    .Select(y => y.downloader)
                    .ToArray());
        _supported = new HashSet<Type>(_all.Keys);
    }

    public IReadOnlySet<Type> SupportedItemTypes => _supported;

    public async Task DownloadIntoFolderAsync(string folder, IItem details, CancellationToken cancellationToken)
    {
        if (!_all.TryGetValue(details.GetType(), out var downloaders))
            throw new NotSupportedException(details.GetType().Name);

        var errors = new List<Exception>();
        foreach (var itemDownloader in downloaders)
        {
            try
            {
                await itemDownloader.DownloadIntoFolderAsync(folder, details, cancellationToken).ConfigureAwait(false);
                return;
            }
            catch(Exception e)
            {
                if (downloaders.Length == 1)
                    throw;
                errors.Add(e);
            }
        }

        if (errors.Any())
            throw new AggregateException(errors);
    }
}