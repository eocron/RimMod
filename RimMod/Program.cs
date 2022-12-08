using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.Logging.Console;
using RimMod.IoC;
using RimMod.Synchronization;

namespace RimMod
{

    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .AddCommandLine(args)
                .Build();

            using var cts = new CancellationTokenSource();
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
            ApplicationConfigurator.Configure(collection);
            using var container = new Container(rules=> rules.WithMicrosoftDependencyInjectionRules()).WithDependencyInjectionAdapter(collection);
            ApplicationConfigurator.Configure(container);

            using var mainScope = container.CreateScope();
            {
                try
                {
                    await container.GetRequiredService<IItemSynchronizationRunner>().RunAsync(cts.Token);
                }
                catch (Exception e)
                {
                    container.GetRequiredService<ILogger<Program>>().LogError(e.ToString());
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
