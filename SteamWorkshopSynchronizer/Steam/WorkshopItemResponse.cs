using System.Runtime.Serialization;

namespace SteamWorkshopSynchronizer.Steam
{
    [DataContract]
    public class WorkshopItemResponse
    {
        [DataMember(Name = "publishedfiledetails")]
        public WorkshopItemDetails[] Details { get; set; }
    }
}