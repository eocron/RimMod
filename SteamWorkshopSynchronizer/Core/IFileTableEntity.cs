namespace SteamWorkshopSynchronizer.Core
{
    public interface IFileTableEntity : ITableEntity
    {
        string EscapedTitle { get; set; }
    }
}