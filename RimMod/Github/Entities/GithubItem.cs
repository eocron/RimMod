using System.Runtime.Serialization;
using RimMod.Settings;
using RimMod.Synchronization;

namespace RimMod.Github.Entities
{
    [DataContract(Namespace = ApplicationConst.EntitiesNamespace, Name = "GithubItem")]
    public class GithubItem : Item<GithubItemId>
    {
        public GithubItem(GithubItemId id, string name, string version) : base(id, name, version)
        {
        }
    }
}