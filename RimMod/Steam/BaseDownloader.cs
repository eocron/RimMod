using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RimMod.OnlineDownloaders;
using RimMod.Synchronization;

namespace RimMod.Steam;

public abstract class BaseDownloader<TId, TItem> : IItemDownloader
{
    public IReadOnlySet<Type> SupportedItemTypes { get; }

    protected BaseDownloader()
    {
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

    protected abstract Task OnDownloadIntoFolderAsync(string folder, TItem item, CancellationToken cancellationToken);
}