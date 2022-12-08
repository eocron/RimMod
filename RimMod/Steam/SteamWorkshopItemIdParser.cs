using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RimMod.Steam.Entities;
using RimMod.Synchronization;

namespace RimMod.Steam
{
    public sealed class SteamWorkshopItemIdParser : IIdParser<SteamWorkshopItemId>
    {
        private static readonly Regex SteamWorkshopItemIdPattern = new Regex(
            @"steamcommunity.+?\/\?id\=(?<fileId>\d+)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public IEnumerable<SteamWorkshopItemId> Parse(string text)
        {
            return SteamWorkshopItemIdPattern.Matches(text)
                .Cast<Match>()
                .Select(x => x.Groups["fileId"].Value)
                .Select(x=> new SteamWorkshopItemId(long.Parse(x)));
        }
    }
}