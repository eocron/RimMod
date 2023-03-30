namespace SteamWorkshopSynchronizer.Settings
{
    public class SteamWorkshopSynchronizerSettings
    {
        public SynchronizationMode Mode { get; set; }
        public long[] AllFileIds { get; set; }
        public string TargetFolderPath { get; set; }
    }
}