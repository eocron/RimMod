using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RimMod
{

    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .Add(new SimpleTextFileSource("links.txt", "Mods:Links"))
                .AddCommandLine(args)
                .Build();

            using var cts = new CancellationTokenSource();
            var collection = new ServiceCollection();
            collection.AddLogging(x =>
            {
                x.ClearProviders();
                x.AddConsole();
                x.AddFilter("System.Net.Http.HttpClient", x => false);
            });
            collection.AddSingleton(config);
            RegisterAll(collection);
            using (var provider = collection.BuildServiceProvider())
            {
                try
                {
                    await provider.GetRequiredService<ISteamModDownloader>().RunAsync(cts.Token);
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

        static void RegisterAll(IServiceCollection services)
        {
            services.AddSingleton<ISteamModDownloader, SteamModDownloader>();
            services.AddHttpClient();
            services.AddSingleton<ModDownloadSettings>(x => x.GetRequiredService<IConfigurationRoot>().GetSection("Mods").Get<ModDownloadSettings>());
            services.AddSingleton<IGameModDirectoryDetector, GameModDirectoryDetector>();
        }
    }
}
