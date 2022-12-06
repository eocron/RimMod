using System;
using System.Net.Http;
using App.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RimMod.Helpers;
using RimMod.OnlineDownloaders;
using RimMod.Settings;
using RimMod.Workshop;

namespace RimMod.IoC
{
    public static class ApplicationConfigurator
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddSingleton<IMetrics>(x =>
                new MetricsBuilder()
                    .Configuration
                    .Configure(options =>
                    {
                        options.DefaultContextLabel = ApplicationConst.Context;
                        options.Enabled = true;
                        options.ReportingEnabled = true;
                    })
                    .Build());

            services.AddHttpClient(ApplicationConst.SteamHttpClientName, x => { x.BaseAddress = new Uri("https://api.steampowered.com/"); });
            services.AddSingleton(x => ApplicationSettingsReader.Read(x.GetRequiredService<IConfigurationRoot>()));
            services.AddSingleton(x =>
                new SteamWorkshopProvider(
                    x.GetRequiredService<IHttpClientFactory>(),
                    x.GetRequiredService<ApplicationSettings>().WorkshopItemIds));
            services.AddSingleton(x => new LocalWorkshopProvider(
                x.GetRequiredService<ApplicationSettings>().ModFolder));
            services.AddSingleton(x =>
                new LocalWorkshopManager(
                    x.GetRequiredService<ApplicationSettings>().ModFolder));
            services.AddSingleton<IWorkshopSynchronizationManager>(x =>
                new WorkshopSynchronizationManager(
                    x.GetRequiredService<LocalWorkshopManager>(),
                    x.GetRequiredService<IWorkshopItemDownloader>()));

            services.AddSingleton<IWorkshopSynchronizationRunner>(x =>
                new WorkshopSynchronizationRunner(
                    x.GetRequiredService<SteamWorkshopProvider>(),
                    x.GetRequiredService<LocalWorkshopProvider>(),
                    x.GetRequiredService<IWorkshopSynchronizationManager>(),
                    x.GetRequiredService<ApplicationSettings>().Mode));

            services.AddSingleton<IWorkshopItemDownloader>(x =>
                new SemaphoreWorkshopItemDownloader(
                    new MonitoredWorkshopItemDownloader(
                        new InfiniteRetryWorkshopItemDownloader(
                            new Vova1234Downloader(
                                x.GetRequiredService<IHttpClientFactory>(),
                                x.GetRequiredService<ILogger<Vova1234Downloader>>()),
                            x.GetRequiredService<ApplicationSettings>().RetryWaitInterval),
                        x.GetRequiredService<IMetrics>()),
                    x.GetRequiredService<ApplicationSettings>().MaxParallelDownloadCount));
        }
    }
}
