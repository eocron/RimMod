using System;
using RimMod.Settings;
using System.Runtime.Serialization;
using RimMod.Synchronization;

namespace RimMod.Github.Entities
{
    [DataContract(Namespace = ApplicationConst.EntitiesNamespace, Name = "GithubItemId")]
    public sealed class GithubItemId : IItemId, IEquatable<GithubItemId>
    {
        [DataMember]
        public string Username { get; private set; }
        [DataMember]
        public string RepositoryName { get; private set; }
        [DataMember]
        public string AssetPath { get; private set; }

        private GithubItemId(){}
        public GithubItemId(string username, string repositoryName, string assetPath)
        {
            Username = username;
            RepositoryName = repositoryName;
            AssetPath = assetPath;
        }

        public bool Equals(GithubItemId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Username == other.Username && RepositoryName == other.RepositoryName && AssetPath == other.AssetPath;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GithubItemId)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Username, RepositoryName, AssetPath);
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}/{2}", Username, RepositoryName, AssetPath);
        }
    }
}