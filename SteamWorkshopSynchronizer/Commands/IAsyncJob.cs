using System.Threading;
using System.Threading.Tasks;

namespace SteamWorkshopSynchronizer.Commands
{
    public interface IAsyncJob
    {
        Task RunAsync(CancellationToken ct);
    }
}