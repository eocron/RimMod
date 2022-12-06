using System.Threading;
using System.Threading.Tasks;

namespace RimMod.Workshop
{
    public interface IWorkshopSynchronizationRunner
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}