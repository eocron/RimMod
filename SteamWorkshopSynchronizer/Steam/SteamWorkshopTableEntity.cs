using System;
using SteamWorkshopSynchronizer.Core;

namespace SteamWorkshopSynchronizer.Steam
{
    public class SteamWorkshopTableEntity : IFileTableEntity
    {
        public string Key { get; set; }
        public string ETag { get; set; }
        public string EscapedTitle { get; set; }
        public long FileId { get; set; }
        public int AppId { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}