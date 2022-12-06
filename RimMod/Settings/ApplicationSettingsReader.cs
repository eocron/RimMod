using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace RimMod.Settings
{
    public static class ApplicationSettingsReader
    {
        public static ApplicationSettings Read(IConfiguration configuration)
        {
            var result = configuration.Get<ApplicationSettings>();
            result.WorkshopItemIds = GetWorkshopItemIdsFromFile(result.ModLinks, CancellationToken.None)
                .Result
                .Concat(result.WorkshopItemIds ?? new List<long>())
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            return result;
        }
        private static async Task<IEnumerable<long>> GetWorkshopItemIdsFromFile(string filePath, CancellationToken ct)
        {
            var text = await File.ReadAllTextAsync(filePath, ct).ConfigureAwait(false);
            return FileIdPattern.Matches(text)
                .Cast<Match>()
                .Select(x => long.Parse(x.Groups["fileId"].Value));
        }

        private static readonly Regex FileIdPattern = new Regex(@"https?\:\/\/.+?\/\?id\=(?<fileId>\d+)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    }
}
