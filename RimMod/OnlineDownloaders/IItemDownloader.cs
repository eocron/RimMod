using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RimMod.Synchronization;

namespace RimMod.OnlineDownloaders
{
    public interface IItemDownloader
    {
        IReadOnlySet<Type> SupportedItemTypes { get; }

        Task DownloadIntoFolderAsync(string folder, IItem details, CancellationToken cancellationToken);
    }
}
