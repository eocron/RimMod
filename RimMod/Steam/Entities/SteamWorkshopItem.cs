using RimMod.Settings;
using RimMod.Synchronization;
using System.Runtime.Serialization;

namespace RimMod.Steam.Entities
{
    [DataContract(Namespace = ApplicationConst.EntitiesNamespace, Name = "SteamItem")]
    public class SteamWorkshopItem : Item<SteamWorkshopItemId>
    {
        [DataMember]
        public WorkshopItemDetails Details { get; set; }
        public SteamWorkshopItem(SteamWorkshopItemId id, string name, string version, WorkshopItemDetails details) : base(id, name, version)
        {
            Details = details;
        }
    }
}