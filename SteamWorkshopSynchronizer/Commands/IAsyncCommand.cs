using System.Threading;
using System.Threading.Tasks;

namespace SteamWorkshopSynchronizer.Commands
{
    public interface IAsyncCommand
    {
        Task RunAsync(CancellationToken ct);
    }
}