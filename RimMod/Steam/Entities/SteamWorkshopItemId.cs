using System;
using RimMod.Settings;
using RimMod.Synchronization;
using System.Runtime.Serialization;

namespace RimMod.Steam.Entities
{
    [DataContract(Namespace = ApplicationConst.EntitiesNamespace, Name = "SteamItemId")]
    public sealed class SteamWorkshopItemId : IItemId , IEquatable<SteamWorkshopItemId>
    {
        [DataMember]
        public long FileId { get; private set; }

        private SteamWorkshopItemId(){}
        public SteamWorkshopItemId(long fileId)
        {
            FileId = fileId;
        }

        public bool Equals(SteamWorkshopItemId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return FileId == other.FileId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SteamWorkshopItemId)obj);
        }

        public override int GetHashCode()
        {
            return FileId.GetHashCode();
        }

        public override string ToString()
        {
            return FileId.ToString();
        }
    }
}