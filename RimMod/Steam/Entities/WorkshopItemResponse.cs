using System.Runtime.Serialization;

namespace RimMod.Steam.Entities
{
    [DataContract]
    public class WorkshopItemResponse
    {
        [DataMember(Name = "publishedfiledetails")]
        public WorkshopItemDetails[] Details { get; set; }
    }
}