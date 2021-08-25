using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RimMod
{
    public static class RimModConfigurator
    {
        public static void RegisterAll(IServiceCollection services)
        {
            services.AddSingleton<ISteamModDownloader, SteamWorkshopDownloader>();
            services.AddHttpClient();
            services.AddSingleton<ModDownloadSettings>(x => x.GetRequiredService<IConfigurationRoot>().Get<ModDownloadSettings>());
        }
    }
}
