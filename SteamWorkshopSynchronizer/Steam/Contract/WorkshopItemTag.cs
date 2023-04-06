using System.Runtime.Serialization;

namespace SteamWorkshopSynchronizer.Steam.Contract
{
    [DataContract(Namespace = SteamNamespaces.Entities, Name = "SteamItemTag")]
    public class WorkshopItemTag
    {
        [DataMember(Name = "tag")]
        public string Tag { get; set; }
    }
}