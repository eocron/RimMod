﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.Logging.Console;
using SteamWorkshopSynchronizer.Core;
using SteamWorkshopSynchronizer.IoC;
using SteamWorkshopSynchronizer.Settings;

namespace SteamWorkshopSynchronizer
{
    public sealed class Program
    {
        public static async Task Main(string[] args)
        {
            var config = ConfigurationReader.Read(args);
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
            using var container = new Container(rules => rules.WithMicrosoftDependencyInjectionRules())
                .WithDependencyInjectionAdapter(collection);
            SteamWorkshopSynchronizerConfigurator.Configure(container, settings);
            using var cts = new CancellationTokenSource();

            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs args)
            {
                args.Cancel = true;
                cts?.Cancel();
            };

            using var mainScope = container.CreateScope();

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
