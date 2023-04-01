using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace SteamWorkshopSynchronizer.Steam.Contract
{
    [DataContract(Namespace = ApplicationConst.EntitiesNamespace, Name = "SteamItemDetails")]
    public class WorkshopItemDetails
    {
        [DataMember(Name = "publishedfileid")]
        public long ItemId { get; set; }
        [DataMember(Name = "title")]
        public string Title { get; set; }
        [DataMember(Name = "time_updated")]
        public long LastUpdatedTimestamp { get; set; }
        [DataMember(Name = "creator_app_id")]
        public int AppId { get; set; }
        [DataMember(Name = "file_size")]
        public long FileSize { get; set; }

        [DataMember(Name = "tags")]
        public WorkshopItemTag[] Tags { get; set; }

        [IgnoreDataMember]
        public string EscapedTitle => Title == null
            ? null
            : string.Join("",
                Title.Split(Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()).ToArray(),
                    StringSplitOptions.RemoveEmptyEntries));
    }
}