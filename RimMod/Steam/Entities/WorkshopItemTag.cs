﻿using RimMod.Settings;
using System.Runtime.Serialization;

namespace RimMod.Steam.Entities
{
    [DataContract(Namespace = ApplicationConst.EntitiesNamespace, Name = "SteamItemTag")]
    public class WorkshopItemTag
    {
        [DataMember(Name = "tag")]
        public string Tag { get; set; }
    }
}