using System;
using System.IO;

namespace RimMod.Helpers
{
    public static class PathHelper
    {
        public static string GetLocalDetailsPath(string workitemFolder)
        {
            return Path.Combine(workitemFolder, "update_info.json");
        }
        public static string GetLocalDetailsPath(string folder, long fileId)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));
            if (fileId <= 0)
                throw new ArgumentOutOfRangeException(nameof(fileId));

            return Path.Combine(folder, fileId.ToString(), "update_info.json");
        }
    }
}
