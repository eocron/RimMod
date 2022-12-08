using System;
using System.Collections.Generic;
using RimMod.Synchronization;

namespace RimMod.Settings
{
    public class ApplicationSettings
    {
        public string ModFolder { get; set; }
        public string ModLinks { get; set; }
        public UpdateMode Mode { get; set; }
        public List<IItemId> ItemIds { get; set; }
        public int MaxParallelDownloadCount { get; set; }
        public TimeSpan RetryWaitInterval { get; set; }
    }
}
