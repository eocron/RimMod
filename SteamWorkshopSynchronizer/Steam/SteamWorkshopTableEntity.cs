using System;

namespace SteamWorkshopSynchronizer.Steam
{
    public class SteamWorkshopTableEntity : ITableEntity
    {
        public string Key { get; set; }
        public DateTime Modified { get; set; }
        public string EscapedTitle { get; set; }
        public long FileId { get; set; }
        public long AppId { get; set; }
    }
}