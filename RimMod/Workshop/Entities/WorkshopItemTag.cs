using System.Runtime.Serialization;

namespace RimMod.Workshop.Entities
{
    [DataContract]
    public class WorkshopItemTag
    {
        [DataMember(Name = "tag")]
        public string Tag { get; set; }
    }
}