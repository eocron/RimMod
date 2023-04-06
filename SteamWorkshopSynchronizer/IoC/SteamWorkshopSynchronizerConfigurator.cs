using System;
using System.Net.Http;
using DryIoc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SteamWorkshopSynchronizer.Core;
using SteamWorkshopSynchronizer.Folder;
using SteamWorkshopSynchronizer.Settings;
using SteamWorkshopSynchronizer.Steam;

namespace SteamWorkshopSynchronizer.IoC
{
    public static class SteamWorkshopSynchronizerConfigurator
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddHttpClient(SteamWorkshopSynchronizerConst.SteamApiHttpClientName, x => { x.BaseAddress = new Uri("https://api.steampowered.com/"); });
            services.AddHttpClient(SteamWorkshopSynchronizerConst.SteamCmdDownloadHttpClientName);
        }

        public static void Configure(IContainer container, SteamWorkshopSynchronizerSettings settings)
        {
            var sourceName = "source";
            var targetName = "target";


            container
                .RegisterSingleton<IAsyncJob>(
                    r => new WindowsSteamCmdDownloadJob(
                        r.Resolve<IHttpClientFactory>(),
                        SteamWorkshopSynchronizerConst.SteamCmdDownloadHttpClientName,
                        r.Resolve<ILogger<WindowsSteamCmdDownloadJob>>(),
                        settings.SteamCmd.DownloadLink,
                        settings.SteamCmd.FolderPath,
                        settings.SteamCmd.TempFolderPath,
                        settings.SteamCmd.ForceUpdate),
                    name: nameof(WindowsSteamCmdDownloadJob))
                .RegisterSingleton<ISteamClient>(
                    r =>
                        new SemaphoreSteamClient(
                            new SteamClient(
                                settings.SteamCmd.Credentials,
                                settings.SteamCmd.FolderPath,
                                r.Resolve<ILogger<SteamClient>>()),
                            settings.SteamCmd.MaxParallelInstanceCount))
                .RegisterSingleton<IFolderUpdater<SteamWorkshopTableEntity>>(
                    r => new SteamWorkshopTableEntityUpdater(
                        r.Resolve<ISteamClient>()))
                .RegisterSingleton<FolderTableEntityManager<SteamWorkshopTableEntity>>(
                    r => new FolderTableEntityManager<SteamWorkshopTableEntity>(
                        settings.TargetFolderPath,
                        r.Resolve<ILogger<FolderTableEntityManager<SteamWorkshopTableEntity>>>(),
                        r.Resolve<IFolderUpdater<SteamWorkshopTableEntity>>()))
                .RegisterSingleton<ITableEntityManager<SteamWorkshopTableEntity>>(
                    r => new MonitoredTableEntityManager<SteamWorkshopTableEntity>(
                        r.Resolve<FolderTableEntityManager<SteamWorkshopTableEntity>>(),
                        r.Resolve<ILogger<FolderTableEntityManager<SteamWorkshopTableEntity>>>()),
                    name: targetName)
                .RegisterSingleton<ITableEntityProvider<SteamWorkshopTableEntity>>(
                    r => r.Resolve<FolderTableEntityManager<SteamWorkshopTableEntity>>(),
                    name: targetName)
                .RegisterSingleton<ITableEntityProvider<SteamWorkshopTableEntity>>(r =>
                        new SteamWorkshopTableEntityProvider(
                            r.Resolve<IHttpClientFactory>(),
                            SteamWorkshopSynchronizerConst.SteamApiHttpClientName,
                            settings.AllFileIds),
                    name: sourceName)
                .RegisterSingleton<IAsyncJob>(
                    r => new TableEntitySynchronizationAsyncJob<SteamWorkshopTableEntity>(
                        r.Resolve<ITableEntityProvider<SteamWorkshopTableEntity>>(sourceName),
                        r.Resolve<ITableEntityProvider<SteamWorkshopTableEntity>>(targetName),
                        new ErrorSuppressingTableEntityManager<SteamWorkshopTableEntity>(
                            r.Resolve<ITableEntityManager<SteamWorkshopTableEntity>>(targetName)),
                        settings.Mode,
                        true,
                        r.Resolve<ILogger<TableEntitySynchronizationAsyncJob<SteamWorkshopTableEntity>>>()),
                    nameof(TableEntitySynchronizationAsyncJob<SteamWorkshopTableEntity>))
                .RegisterSingleton<IAsyncJob>(
                    r => new CompoundAsyncJob(
                        false,
                        new RestartUntilSuccessAsyncJob(
                            r.Resolve<IAsyncJob>(nameof(WindowsSteamCmdDownloadJob)),
                            r.Resolve<ILogger<WindowsSteamCmdDownloadJob>>(),
                            TimeSpan.FromSeconds(1)),
                        r.Resolve<IAsyncJob>(
                            nameof(TableEntitySynchronizationAsyncJob<SteamWorkshopTableEntity>))));
        }
    }
}