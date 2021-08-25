using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RimMod
{
    public class GameModDirectoryDetector : IGameModDirectoryDetector
    {
        private readonly ILogger<GameModDirectoryDetector> _logger;

        public GameModDirectoryDetector(ILogger<GameModDirectoryDetector> logger)
        {
            _logger = logger;
        }

        public async Task<string> Detect(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to find game directory.");
            foreach (var d in DriveInfo.GetDrives())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = d.RootDirectory.GetFiles("RimWorldWin64.exe", SearchOption.AllDirectories).FirstOrDefault();
                if (result != null)
                {
                    _logger.LogInformation($"Found game directory {result.DirectoryName}");
                    return Path.Combine(result.DirectoryName, "Mods");
                }
            }
            return null;
        }
    }
}
