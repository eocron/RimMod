using System.Runtime.Serialization;

namespace RimMod.Steam.Entities
{
    [DataContract]
    public class SteamApiResponse
    {
        [DataMember(Name = "response")]
        public WorkshopItemResponse Response { get; set; }
    }
}