using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SteamWorkshopSynchronizer.Core;

namespace SteamWorkshopSynchronizer.Steam
{
    public sealed class SteamTargetDigestGenerationAsyncJob : IAsyncJob
    {
        private readonly ITableEntityProvider<SteamWorkshopTableEntity> _targetProvider;
        private readonly string _digestFilePath;
        private readonly ILogger _logger;
        private const string SteamWorkshopLinkPattern = "https://steamcommunity.com/sharedfiles/filedetails/?id={0}\t// {1}\n";

        public SteamTargetDigestGenerationAsyncJob(
            ITableEntityProvider<SteamWorkshopTableEntity> targetProvider,
            string digestFilePath,
            ILogger logger)
        {
            _targetProvider = targetProvider;
            _digestFilePath = digestFilePath;
            _logger = logger;
        }
        
        
        public async Task RunAsync(CancellationToken ct)
        {
            var all = await _targetProvider.GetAllEntitiesAsync(ct).ConfigureAwait(false);
            var digest = GenerateDigest(all);
            await File.WriteAllTextAsync(_digestFilePath, digest, ct).ConfigureAwait(false);
            _logger.LogInformation("Created synchronization digest at {filePath}", _digestFilePath);
        }

        private string GenerateDigest(IEnumerable<SteamWorkshopTableEntity> entities)
        {
            var sb = new StringBuilder();
            foreach (var e in entities.OrderBy(x=> x.EscapedTitle).ThenBy(x=> x.FileId))
            {
                sb.AppendFormat(SteamWorkshopLinkPattern, e.FileId, e.EscapedTitle);
            }

            return sb.ToString();
        }
    }
}