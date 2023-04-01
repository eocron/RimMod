using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace SteamWorkshopSynchronizer.Settings
{
    public static class SteamWorkshopSynchronizerSettingsReader
    {
        public static SteamWorkshopSynchronizerSettings Read(IConfiguration configuration)
        {
            var cfg = configuration.Get<SteamWorkshopSynchronizerConfig>();
            var fileIds = new List<long>();
            if (cfg.WorkshopItemsFilePath != null)
            {
                fileIds.AddRange(File.ReadAllLines(cfg.WorkshopItemsFilePath).Select(TryParseWorkshopFileId));
            }

            if (cfg.AllFileIds != null)
            {
                fileIds.AddRange(cfg.AllFileIds);
            }
            var allFileIds = fileIds.Distinct().Where(x => x > 0).OrderBy(x => x).ToArray();
            return new SteamWorkshopSynchronizerSettings
            {
                AllFileIds = allFileIds,
                Mode = cfg.Mode,
                SteamCmd = cfg.SteamCmd,
                TargetFolderPath = cfg.TargetFolderPath
            };
        }

        private static long TryParseWorkshopFileId(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;

            var match = Regex.Match(input, @"id=(?<id>\d+)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
            if (match.Success)
                return long.Parse(match.Groups["id"].Value);
            return 0;
        }
    }
}