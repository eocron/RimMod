using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RimMod.Github.Entities;
using RimMod.Synchronization;

namespace RimMod.Github
{
    public sealed class GithubItemIdParser : IIdParser<GithubItemId>
    {
        private static readonly Regex SteamWorkshopItemIdPattern = new Regex(
            @"github\.com[\/\\](?<user_name>[^\/\\]+)[\/\\](?<repo_name>[^\/\\]+)([\/\\](?<assetPath>.+))?",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public IEnumerable<GithubItemId> Parse(string text)
        {
            return SteamWorkshopItemIdPattern.Matches(text)
                .Cast<Match>()
                .Select(x => new GithubItemId(x.Groups["user_name"].Value, x.Groups["repo_name"].Value, x.Groups["assetPath"].Value));
        }
    }
}