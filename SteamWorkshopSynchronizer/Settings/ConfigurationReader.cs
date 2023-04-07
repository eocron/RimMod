using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using SteamWorkshopSynchronizer.Settings.Configs;

namespace SteamWorkshopSynchronizer.Settings
{
    public static class ConfigurationReader
    {
        public static IConfiguration Read(string[] args)
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{GetEnvironment(args)}.json", true)
                .AddCommandLine(args, SwitchMappings)
                .Build();
        }
        
        private static string GetEnvironment(string[] args)
        {
            return new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build()["env"];
        }

        private static readonly IDictionary<string, string> SwitchMappings = new Dictionary<string, string>
        {
            { "--target", nameof(SteamWorkshopSynchronizerConfig.TargetFolderPath) },
            { "--mode", nameof(SteamWorkshopSynchronizerConfig.Mode) },
            { "--source", nameof(SteamWorkshopSynchronizerConfig.WorkshopItemsFilePath) }
        };
    }
}