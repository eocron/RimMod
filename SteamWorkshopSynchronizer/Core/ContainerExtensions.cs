using System;
using DryIoc;

namespace SteamWorkshopSynchronizer.Core
{
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
    }
}