using System.Threading;
using System.Threading.Tasks;

namespace RimMod
{
    public interface IGameModDirectoryDetector
    {
        Task<string> Detect(CancellationToken cancellationToken);
    }
}