using System;

namespace SteamWorkshopSynchronizer.Settings
{
    [Flags]
    public enum SynchronizationMode
    {
        Create = 1 << 0,
        Update = 1 << 1,
        Delete = 1 << 2
    }
}