using System.Runtime.Serialization;

namespace RimMod.Github.Entities;

[DataContract]
public class GithubReleaseAsset
{
    [DataMember(Name = "id")]
    public int AssetId { get; set; }
    [DataMember(Name = "content_type")]
    public string ContentType { get; set; }
    [DataMember(Name = "size")]
    public int Size { get; set; }
    [DataMember(Name = "browser_download_url")]
    public string DownloadLink { get; set; }
    [DataMember(Name = "name")]
    public string Name { get; set; }
}