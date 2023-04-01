using System.Threading;
using System.Threading.Tasks;

namespace SteamWorkshopSynchronizer.Core
{
    public interface IAsyncJob
    {
        Task RunAsync(CancellationToken ct);
    }
}