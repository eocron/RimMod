using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.Logging.Console;
using SteamWorkshopSynchronizer.Commands;
using SteamWorkshopSynchronizer.IoC;
using SteamWorkshopSynchronizer.Settings;

namespace SteamWorkshopSynchronizer
{

    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .AddCommandLine(args)
                .Build();
            
            var collection = new ServiceCollection();
            collection.AddLogging(x =>
            {
                x.ClearProviders();
                x.AddConfiguration(config.GetSection("Logging"));
                x.AddSimpleConsole(o =>
                {
                    o.SingleLine = true;
                    o.ColorBehavior = LoggerColorBehavior.Enabled;
                });
            });

            collection.AddSingleton(config);
            var settings = SteamWorkshopSynchronizerSettingsReader.Read(config);
            SteamWorkshopSynchronizerConfigurator.Configure(collection);
            using var container = new Container(rules=> rules.WithMicrosoftDependencyInjectionRules()).WithDependencyInjectionAdapter(collection);
            SteamWorkshopSynchronizerConfigurator.Configure(container, settings);
            using var cts = new CancellationTokenSource();
            using var mainScope = container.CreateScope();
            {
                try
                {
                    await container.Resolve<IAsyncJob>().RunAsync(cts.Token);
                }
                catch (Exception e)
                {
                    container.Resolve<ILogger<Program>>().LogError(e.ToString());
                    throw;
                }
                finally
                {
                    cts.Cancel();
                }
            }
        }
    }
}
