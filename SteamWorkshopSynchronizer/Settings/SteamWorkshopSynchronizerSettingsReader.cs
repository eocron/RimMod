using Microsoft.Extensions.Configuration;

namespace SteamWorkshopSynchronizer.Settings
{
    public static class SteamWorkshopSynchronizerSettingsReader
    {
        public static SteamWorkshopSynchronizerSettings Read(IConfiguration configuration)
        {
            return configuration.Get<SteamWorkshopSynchronizerSettings>();
        }
    }
}