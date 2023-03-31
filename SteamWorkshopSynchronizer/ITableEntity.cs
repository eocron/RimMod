using System;

namespace SteamWorkshopSynchronizer
{
    public interface ITableEntity
    {
        string Key { get; set; }
        
        string ETag { get; set; }
    }
}