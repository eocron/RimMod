using System.Runtime.Serialization;

namespace SteamWorkshopSynchronizer.Steam.Contract
{
    [DataContract]
    public class WorkshopItemResponse
    {
        [DataMember(Name = "publishedfiledetails")]
        public WorkshopItemDetails[] Details { get; set; }
    }
}