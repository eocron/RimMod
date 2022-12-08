using System;
using App.Metrics;
using DryIoc;
using Microsoft.Extensions.DependencyInjection;
using RimMod.OnlineDownloaders;
using RimMod.Settings;
using RimMod.Synchronization;

namespace RimMod.Helpers;

public static class ContainerExtensions
{
    public static IContainer RegisterSingleton<TInterface>(this IContainer container, Func<IResolver, TInterface> provider, object name = null)
    {
        container.RegisterDelegate<TInterface>(provider, reuse: Reuse.Singleton, serviceKey: name);
        return container;
    }

    public static IContainer RegisterSingleton<TInterface, TImplementation>(this IContainer container, object name = null)
        where TImplementation : TInterface
    {
        container.Register<TInterface, TImplementation>(reuse: Reuse.Singleton, serviceKey: name);
        return container;
    }

    public static IContainer RegisterItemDownloader(this IContainer services, Func<IResolver, IItemDownloader> provider)
    {
        return services
            .RegisterSingleton<IItemDownloader>(x =>
                new SemaphoreItemDownloader(
                    new MonitoredItemDownloader(
                        new InfiniteRetryItemDownloader(
                            provider(x),
                            x.GetRequiredService<ApplicationSettings>().RetryWaitInterval),
                        x.GetRequiredService<IMetrics>()),
                    x.GetRequiredService<ApplicationSettings>().MaxParallelDownloadCount));
    }

    public static IContainer RegisterRawItemProvider<TId, TItem>(this IContainer services, Func<IResolver, IItemProvider<TId, TItem>> provider, string name = null)
    {
        return services
            .RegisterSingleton(provider)
            .RegisterSingleton<IRawItemProvider>(x => new RawItemProvider<TId, TItem>(x.Resolve<IItemProvider<TId, TItem>>()), name);
    }

    public static IContainer RegisterRawIdParser<TParser, TId>(this IContainer services, string name = null) where TParser : class, IIdParser<TId>
    {
        return services
            .RegisterSingleton<IIdParser<TId>, TParser>()
            .RegisterSingleton<IRawIdParser>(x => new RawIdParser<TId>(x.GetRequiredService<IIdParser<TId>>()), name);
    }
}