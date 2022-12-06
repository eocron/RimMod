using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Timer;
using RimMod.OnlineDownloaders;
using RimMod.Settings;
using RimMod.Workshop.Entities;

namespace RimMod.Helpers
{
    public sealed class MonitoredWorkshopItemDownloader : IWorkshopItemDownloader
    {
        private readonly IWorkshopItemDownloader _inner;
        private readonly IMetrics _metrics;

        private static readonly TimerOptions Timer = new TimerOptions
        {
            Context = ApplicationConst.Context,
            MeasurementUnit = Unit.Events,
            DurationUnit = TimeUnit.Milliseconds,
            Name = "download",
            RateUnit = TimeUnit.Milliseconds
        };

        public MonitoredWorkshopItemDownloader(IWorkshopItemDownloader inner, IMetrics metrics)
        {
            _inner = inner;
            _metrics = metrics;
        }

        public async Task DownloadIntoFolderAsync(string folder, WorkshopItemDetails details, CancellationToken cancellationToken)
        {
            using (_metrics.Measure.Timer.Time(Timer))
            {
                await _inner.DownloadIntoFolderAsync(folder, details, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}