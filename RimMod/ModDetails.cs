using System;

namespace RimMod
{
    public class ModDetails : IComparable<ModDetails>
    {
        public long publishedfileid { get; set; }
        public string title { get; set; }
        public long time_created { get; set; }
        public long time_updated { get; set; }

        public int CompareTo(ModDetails other)
        {
            return time_updated.CompareTo(other.time_updated);
        }
    }
}