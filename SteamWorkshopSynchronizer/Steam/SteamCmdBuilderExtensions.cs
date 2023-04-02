using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace SteamWorkshopSynchronizer.Steam
{
    public static class SteamCmdBuilderExtensions
    {
        public static SteamCmdBuilder AddCommand(this SteamCmdBuilder builder, string command, params object[] args)
        {
            builder.AddCommand(string.Format(command, args));
            return builder;
        }

        public static SteamCmdBuilder WithLogger(this SteamCmdBuilder builder, ILogger logger)
        {
            builder.Logger = logger;
            return builder;
        }

        public static SteamCmdBuilder WithExePath(this SteamCmdBuilder builder, string exePath)
        {
            builder.SteamCmdExecutablePath = exePath;
            return builder;
        }

        public static SteamCmdBuilder WithOutputRegex(this SteamCmdBuilder builder,string pattern, Action<Match> onMatch)
        {
            builder.AddPerLineOutputMatcher(pattern, onMatch);
            return builder;
        }
    }
}