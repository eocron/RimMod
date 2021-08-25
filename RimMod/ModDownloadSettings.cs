using System.Collections.Generic;

namespace RimMod
{
    public class ModDownloadSettings
    {
        public List<string> Links { get; set; }
        public string SteamWorkshopLink { get; set; }
        public string OutputFolder { get; set; }
        public bool CleanOutOtherMods { get; set; }
        public bool OverwriteFiles { get; set; }
    }
}
