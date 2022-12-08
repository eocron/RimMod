using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RimMod.Synchronization;

namespace RimMod.Settings
{
    public static class ApplicationSettingsReader
    {
        public static ApplicationSettings Read(IConfiguration configuration, IRawIdParser parser)
        {
            var result = configuration.Get<ApplicationSettings>();
            result.ItemIds = GetItemIdsFromFile(result.ModLinks, parser, CancellationToken.None)
                .Result
                .Distinct()
                .ToList();
            return result;
        }
        private static async Task<IEnumerable<IItemId>> GetItemIdsFromFile(string filePath, IRawIdParser parser, CancellationToken ct)
        {
            var text = await File.ReadAllTextAsync(filePath, ct).ConfigureAwait(false);
            return parser.Parse(text);
        }
    }
}
