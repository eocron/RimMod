using System;

namespace SteamWorkshopSynchronizer
{
    public interface ITableEntity
    {
        string Key { get; set; }
        
        string EscapedTitle { get; set; }
        
        DateTime Modified { get; set; }
    }
}