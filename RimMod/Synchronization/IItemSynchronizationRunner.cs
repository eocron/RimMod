using System.Threading;
using System.Threading.Tasks;

namespace RimMod.Synchronization
{
    public interface IItemSynchronizationRunner
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}