using System;
using System.Net.Http;
using DryIoc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SteamWorkshopSynchronizer.Commands;
using SteamWorkshopSynchronizer.Folder;
using SteamWorkshopSynchronizer.Settings;
using SteamWorkshopSynchronizer.Steam;

namespace SteamWorkshopSynchronizer.IoC
{
    public static class SteamWorkshopSynchronizerConfigurator
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddHttpClient(ApplicationConst.SteamHttpClientName, x => { x.BaseAddress = new Uri("https://api.steampowered.com/"); });
        }

        public static void Configure(IContainer container, SteamWorkshopSynchronizerSettings settings)
        {
            var sourceName = "source";
            var targetName = "target";
            
            container.RegisterSingleton<FolderTableEntityManager<SteamWorkshopTableEntity>>(
                r => new FolderTableEntityManager<SteamWorkshopTableEntity>(settings.TargetFolderPath,
                    r.Resolve<ILogger<FolderTableEntityManager<SteamWorkshopTableEntity>>>()));

            container.RegisterSingleton<ITableEntityManager<SteamWorkshopTableEntity>>(
                r => r.Resolve<FolderTableEntityManager<SteamWorkshopTableEntity>>(),
                name: targetName);

            container.RegisterSingleton<ITableEntityProvider<SteamWorkshopTableEntity>>(
                r => r.Resolve<FolderTableEntityManager<SteamWorkshopTableEntity>>(),
                name: targetName);
            
            container.RegisterSingleton<ITableEntityProvider<SteamWorkshopTableEntity>>(r =>
                    new SteamWorkshopTableEntityProvider(
                        r.Resolve<IHttpClientFactory>(),
                        ApplicationConst.SteamHttpClientName,
                        settings.AllFileIds),
                name: sourceName);
            
            container.RegisterSingleton<IAsyncJob>(
                r => new TableEntitySynchronizationAsyncCommand<SteamWorkshopTableEntity>(
                    r.Resolve<ITableEntityProvider<SteamWorkshopTableEntity>>(sourceName),
                    r.Resolve<ITableEntityProvider<SteamWorkshopTableEntity>>(targetName),
                    r.Resolve<ITableEntityManager<SteamWorkshopTableEntity>>(targetName),
                    settings.Mode,
                    r.Resolve<ILogger<TableEntitySynchronizationAsyncCommand<SteamWorkshopTableEntity>>>()));
        }
    }
}