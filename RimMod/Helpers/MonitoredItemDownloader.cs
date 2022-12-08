using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Timer;
using RimMod.OnlineDownloaders;
using RimMod.Settings;
using RimMod.Synchronization;

namespace RimMod.Helpers
{
    public sealed class MonitoredItemDownloader : IItemDownloader
    {
        private static readonly TimerOptions Timer = new TimerOptions
        {
            Context = ApplicationConst.Context,
            MeasurementUnit = Unit.Events,
            DurationUnit = TimeUnit.Milliseconds,
            Name = "download",
            RateUnit = TimeUnit.Milliseconds
        };

        private readonly IItemDownloader _inner;
        private readonly IMetrics _metrics;

        public MonitoredItemDownloader(IItemDownloader inner, IMetrics metrics)
        {
            _inner = inner;
            _metrics = metrics;
        }

        public IReadOnlySet<Type> SupportedItemTypes => _inner.SupportedItemTypes;

        public async Task DownloadIntoFolderAsync(string folder, IItem item, CancellationToken cancellationToken)
        {
            using (_metrics.Measure.Timer.Time(Timer))
            {
                await _inner.DownloadIntoFolderAsync(folder, item, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}