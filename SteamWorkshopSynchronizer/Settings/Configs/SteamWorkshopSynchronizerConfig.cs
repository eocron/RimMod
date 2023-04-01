namespace SteamWorkshopSynchronizer.Settings.Configs
{
    public class SteamWorkshopSynchronizerConfig
    {
        public SynchronizationMode Mode { get; set; }
        public long[] AllFileIds { get; set; }
        public string WorkshopItemsFilePath { get; set; }
        public string TargetFolderPath { get; set; }
        public SteamCmdSettings SteamCmd { get; set; }
    }
}