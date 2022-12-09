using System.Runtime.Serialization;
using RimMod.Settings;
using RimMod.Synchronization;

namespace RimMod.Github.Entities
{
    [DataContract(Namespace = ApplicationConst.EntitiesNamespace, Name = "GithubItem")]
    public class GithubItem : Item<GithubItemId>
    {
        [DataMember]
        public string DownloadLink { get; private set; }

        private GithubItem(){}
        public GithubItem(GithubItemId id, string name, string version, string downloadLink) : base(id, name, version)
        {
            DownloadLink = downloadLink;
        }
    }
}