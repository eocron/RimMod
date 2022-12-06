using Newtonsoft.Json;
using System;

namespace RimMod.Settings
{
    [Flags]
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum UpdateMode : int
    {
        Undefined = 0,
        Create = 1 << 0,
        Update = 1 << 1,
        Delete = 1 << 2
    }
}
