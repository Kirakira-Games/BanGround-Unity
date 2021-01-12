using System.Collections.Generic;
using System.IO;

namespace BanGround
{
    public static class KiraPath
    {
        public static string Combine(params string[] paths)
        {
            return Path.Combine(paths).Replace('\\', '/');
        }

        public static string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path).Replace('\\', '/');
        }

        public static string GetFileName(string path)
        {
            return Path.GetFileName(path).Replace('\\', '/');
        }
    }
}