using System.Runtime.Serialization;
using DryIoc.ImTools;

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

        public string GetFolderName()
        {
            return Id?.GetFolderName();
        }

        protected Item(){}

        protected Item(TId id, string name, string version)
        {
            Id = id;
            Name = name;
            Version = version;
        }
    }
}