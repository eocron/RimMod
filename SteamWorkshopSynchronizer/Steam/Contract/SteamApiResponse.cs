using System.Runtime.Serialization;

namespace SteamWorkshopSynchronizer.Steam.Contract
{
    [DataContract]
    public class SteamApiResponse
    {
        [DataMember(Name = "response")]
        public WorkshopItemResponse Response { get; set; }
    }
}