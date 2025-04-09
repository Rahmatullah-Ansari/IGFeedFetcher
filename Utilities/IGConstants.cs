using System.IO;

namespace FeedFetcher.Utilities
{
    public static class IGConstants
    {
        public static string SessionFileName => GetHomeDirectory().CombinePath("Session.txt");
        public static string ApplicationName => Properties.Resources.NM;
        public static string LicenseFile => GetHomeDirectory().CombinePath("License.txt");
        public static string RootDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private static string GetHomeDirectory()
        {
            var homeDirectory = RootDirectory.CombinePath(ApplicationName);
            if (!Directory.Exists(homeDirectory))
                Directory.CreateDirectory(homeDirectory);
            return homeDirectory;
        }
        public static string CombinePath(this string Root,params string[] paths)
        {
            return Path.Combine(Root,Path.Combine(paths));
        }
    }
}
