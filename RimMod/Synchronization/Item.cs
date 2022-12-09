using System.Runtime.Serialization;

namespace RimMod.Synchronization
{
    [DataContract]
    public abstract class Item<TId> : IItem
        where TId : IItemId
    {
        [DataMember]
        public IItemId Id { get; private set; }
        [DataMember]
        public string Name { get; private set; }
        [DataMember]
        public string Version { get; private set; }

        protected Item(){}

        protected Item(TId id, string name, string version)
        {
            Id = id;
            Name = name;
            Version = version;
        }
    }
}