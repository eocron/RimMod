using System;

namespace RimMod
{
    public class ModDownloadSettings
    {
        public string SteamWorkshopLink { get; set; }
        public string ModFolder { get; set; }
        public string ModLinks { get; set; }
        public UpdateMode Mode { get; set; }
        public TimeSpan JobTimeout { get; set; }
    }
}
