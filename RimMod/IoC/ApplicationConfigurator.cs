using System;
using System.Linq;
using System.Net.Http;
using App.Metrics;
using App.Metrics.Logging;
using DryIoc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RimMod.Github;
using RimMod.Github.Entities;
using RimMod.Helpers;
using RimMod.OnlineDownloaders;
using RimMod.Settings;
using RimMod.Steam;
using RimMod.Steam.Entities;
using RimMod.Synchronization;

namespace RimMod.IoC
{
    public static class ApplicationConfigurator
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddHttpClient(ApplicationConst.SteamHttpClientName, x => { x.BaseAddress = new Uri("https://api.steampowered.com/"); });
            services.AddHttpClient(ApplicationConst.GithubHttpClientName, x =>
            {
                x.BaseAddress = new Uri("https://api.github.com/");
                x.DefaultRequestHeaders.Add("User-Agent", "request");
            });
            services.AddHttpClient(ApplicationConst.Vova1234DownloaderHttpClientName);
        }

        public static void Configure(IContainer container)
        {
            var LocalProviderName = "local";
            var RemoteProviderName = "remote";
            var AggregateDownloaderName = "downloader";
            var AggregateParserName = "parser";
            container
                .RegisterSingleton<IMetrics>(_ =>
                    new MetricsBuilder()
                        .Configuration
                        .Configure(options =>
                        {
                            options.DefaultContextLabel = ApplicationConst.Context;
                            options.Enabled = true;
                            options.ReportingEnabled = true;
                        })
                        .Build())
                .RegisterSingleton(x =>
                    ApplicationSettingsReader.Read(
                        x.GetRequiredService<IConfigurationRoot>(),
                        x.Resolve<ComplexRawIdParser>(AggregateParserName)))
                .RegisterSingleton<ComplexRawIdParser>(x => 
                        new ComplexRawIdParser(
                            x.ResolveMany<IRawIdParser>().ToArray()),
                    name: AggregateParserName)
                .RegisterSingleton<IItemManager>(x =>
                        new LocalItemManager(
                            x.GetRequiredService<ApplicationSettings>().ModFolder),
                    name: LocalProviderName)
                .RegisterSingleton<IItemProvider<IItemId, IItem>>(x =>
                        new LocalItemProvider(
                            x.GetRequiredService<ApplicationSettings>().ModFolder,
                            x.Resolve<ILogger<LocalItemProvider>>()),
                    name: LocalProviderName)
                .RegisterSingleton<IItemProvider<IItemId, IItem>>(x =>
                            new ComplexItemProvider(
                                x.ResolveMany<IRawItemProvider>().ToArray()),
                    name: RemoteProviderName)

                .RegisterSingleton<ComplexItemDownloader>(x => new ComplexItemDownloader(x.ResolveMany<IItemDownloader>().ToArray()), name: AggregateDownloaderName)
                .RegisterSingleton<IItemSynchronizationManager>(x =>
                    new ErrorHandlingSynchronizationManager(
                        new ItemSynchronizationManager(
                            x.Resolve<IItemManager>(LocalProviderName),
                            x.Resolve<ComplexItemDownloader>(AggregateDownloaderName)),
                        x.GetRequiredService<ILogger<ItemSynchronizationManager>>()))
                .RegisterSingleton<IItemSynchronizationRunner>(x =>
                    new ItemSynchronizationRunner(
                        new RawItemProvider<IItemId, IItem>(x.Resolve<IItemProvider<IItemId, IItem>>(RemoteProviderName)),
                        new RawItemProvider<IItemId, IItem>(x.Resolve<IItemProvider<IItemId, IItem>>(LocalProviderName)),
                        x.GetRequiredService<IItemSynchronizationManager>(),
                        x.GetRequiredService<ApplicationSettings>().Mode))


                .RegisterItemDownloader(x=> 
                    new GithubDownloader(
                        x.GetRequiredService<IHttpClientFactory>(),
                        ApplicationConst.GithubHttpClientName,
                        x.GetRequiredService<ILogger<GithubDownloader>>()))
                .RegisterItemDownloader(x =>
                    new Vova1234SteamWorkshopDownloader(
                        x.GetRequiredService<IHttpClientFactory>(),
                        ApplicationConst.Vova1234DownloaderHttpClientName,
                        x.GetRequiredService<ILogger<Vova1234SteamWorkshopDownloader>>()))

                .RegisterGithubSupport()
                .RegisterSteamSupport();

            container
                .RegisterRawIdParser<GithubItemIdParser, GithubItemId>();
        }
        private static IContainer RegisterGithubSupport(this IContainer services)
        {
            return services
                .RegisterRawItemProvider(x => 
                    new GithubItemProvider(
                    x.Resolve<IHttpClientFactory>(),
                    ApplicationConst.GithubHttpClientName,
                    x.Resolve<ApplicationSettings>().ItemIds.OfType<GithubItemId>().ToList()))
                .RegisterRawIdParser<GithubItemIdParser, GithubItemId>();
        }

        private static IContainer RegisterSteamSupport(this IContainer services)
        {
            return services
                .RegisterRawItemProvider(x => 
                    new SteamWorkshopItemProvider(
                        x.Resolve<IHttpClientFactory>(), 
                        ApplicationConst.SteamHttpClientName,
                        x.Resolve<ApplicationSettings>().ItemIds.OfType<SteamWorkshopItemId>().ToList()))
                .RegisterRawIdParser<SteamWorkshopItemIdParser, SteamWorkshopItemId>();
        }
    }
}
