using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SteamWorkshopSynchronizer.Steam
{
    public interface ISteamCmdInstance : IDisposable
    {
        ChannelReader<string> Output { get; }

        Task<int> RunToCompletionAsync(CancellationToken ct);
    }
}