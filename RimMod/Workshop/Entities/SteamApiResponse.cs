using System.Runtime.Serialization;

namespace RimMod.Workshop.Entities
{
    [DataContract]
    public class SteamApiResponse
    {
        [DataMember(Name = "response")]
        public WorkshopItemResponse Response { get; set; }
    }
}