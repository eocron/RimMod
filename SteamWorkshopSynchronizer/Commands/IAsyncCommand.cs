using System.Threading;
using System.Threading.Tasks;

namespace SteamWorkshopSynchronizer
{
    public interface IAsyncCommand
    {
        Task RunAsync(CancellationToken ct);
    }
}