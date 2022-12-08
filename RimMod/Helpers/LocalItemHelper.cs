using System;
using System.IO;
using Newtonsoft.Json;
using RimMod.Synchronization;

namespace RimMod.Helpers
{
    public static class LocalItemHelper
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            Formatting = Formatting.Indented
        };

        public static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }

        public static T Deserialize<T>(string input)
        {
            return (T)JsonConvert.DeserializeObject(input, Settings);
        }

        public static string GetLocalDetailsPath(string folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));
            return Path.Combine(folder, "update_info.json");
        }

        public static string GetLocalDetailsPath(string folder, IItemId itemId)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));
            var str = itemId?.ToString();
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentNullException(nameof(itemId));
            return Path.Combine(folder, str, "update_info.json");
        }

        public static string GetLocalDetailsFolder(string folder, IItemId itemId)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));
            var str = itemId?.ToString();
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentNullException(nameof(itemId));
            return Path.Combine(folder, str);
        }
    }
}
