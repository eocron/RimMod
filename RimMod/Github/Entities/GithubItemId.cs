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
        public string ReleasePath { get; private set; }
        [DataMember]
        public long? ReleaseId { get; private set; }

        private GithubItemId(){}
        public GithubItemId(string username, string repositoryName, string releasePath, long? releaseId)
        {
            Username = username;
            RepositoryName = repositoryName;
            ReleasePath = string.IsNullOrWhiteSpace(releasePath) ? null : releasePath.Trim();
            ReleaseId = releaseId;
        }

        public bool Equals(GithubItemId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Username == other.Username && RepositoryName == other.RepositoryName && ReleasePath == other.ReleasePath && ReleaseId == other.ReleaseId;
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
            return HashCode.Combine(Username, RepositoryName, ReleasePath, ReleaseId);
        }

        public override string ToString()
        {
            return ReleaseId != null ? $"github_{ReleaseId}" : $"{Username}_{RepositoryName}_{ReleasePath}";
        }
    }
}