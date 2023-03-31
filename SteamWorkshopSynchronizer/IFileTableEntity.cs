namespace SteamWorkshopSynchronizer
{
    public interface IFileTableEntity : ITableEntity
    {
        string EscapedTitle { get; set; }
    }
}