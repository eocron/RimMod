using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Console;
using RimMod.Workshop;
using RimMod.IoC;

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
            await using var provider = collection.BuildServiceProvider();
            try
            {
                await provider.GetRequiredService<IWorkshopSynchronizationRunner>().RunAsync(cts.Token);
            }
            catch (Exception e)
            {
                provider.GetRequiredService<ILogger<Program>>().LogError("Error occured: " + e);
                throw;
            }
            finally
            {
                cts.Cancel();
            }
        }
    }
}
