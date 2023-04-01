namespace SteamWorkshopSynchronizer.Core
{
    public interface ITableEntity
    {
        string Key { get; set; }
        
        string ETag { get; set; }
    }
}