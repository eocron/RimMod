namespace RimMod.Synchronization;

public interface IItem
{
    IItemId Id { get; }

    string Name { get; }

    string Version { get; }
    string GetFolderName();
}