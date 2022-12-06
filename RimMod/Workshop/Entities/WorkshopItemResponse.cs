using System.Runtime.Serialization;

namespace RimMod.Workshop.Entities
{
    [DataContract]
    public class WorkshopItemResponse
    {
        [DataMember(Name = "publishedfiledetails")]
        public WorkshopItemDetails[] Details { get; set; }
    }
}