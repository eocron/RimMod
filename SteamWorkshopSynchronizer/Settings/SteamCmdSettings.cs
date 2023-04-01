using System;
using SteamWorkshopSynchronizer.Steam;

namespace SteamWorkshopSynchronizer.Settings
{
    public class SteamCmdSettings
    {
        public SteamClientCredentials Credentials { get; set; }
        public string TempFolderPath { get; set; }
        public string FolderPath { get; set; }
        public Uri DownloadLink { get; set; }
        public bool ForceUpdate { get; set; }
        public int MaxParallelInstanceCount { get; set; }
    }
}