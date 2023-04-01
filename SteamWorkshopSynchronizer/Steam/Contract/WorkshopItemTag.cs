using System.Runtime.Serialization;

namespace SteamWorkshopSynchronizer.Steam.Contract
{
    [DataContract(Namespace = ApplicationConst.EntitiesNamespace, Name = "SteamItemTag")]
    public class WorkshopItemTag
    {
        [DataMember(Name = "tag")]
        public string Tag { get; set; }
    }
}