using System;
using System.Runtime.Serialization;

namespace RimMod.Github.Entities;

[DataContract]
public class GithubReleaseResponse
{
    [DataMember(Name = "id")]
    public long Id { get; set; }

    [DataMember(Name = "published_at")]
    public DateTime PublishedAt { get; set; }

    [DataMember(Name = "assets")]
    public GithubReleaseAsset[] Assets { get; set; }
}