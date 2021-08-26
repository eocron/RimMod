using System;

namespace RimMod
{
    public class ModDetails
    {
        public long publishedfileid { get; set; }
        public string title { get; set; }
        public long time_updated { get; set; }

        public override bool Equals(object obj)
        {
            var other = (ModDetails)obj;
            return publishedfileid.Equals(other.publishedfileid) && time_updated.Equals(other.time_updated);
        }

        public override int GetHashCode()
        {
            return publishedfileid.GetHashCode() ^ time_updated.GetHashCode();
        }
    }
}