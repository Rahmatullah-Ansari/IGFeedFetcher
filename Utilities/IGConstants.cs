using System.IO;

namespace FeedFetcher.Utilities
{
    public static class IGConstants
    {
        public static string SessionFileName => GetHomeDirectory().CombinePath("Session.txt");
        public static string ApplicationName => Properties.Resources.NM;
        public static string UserIdAPI => "https://app.autolikesig.com/api/instausername";
        public static string PostAPI { get; set; } = "https://api.gaia.proceedinteractive.com/igServices/ig/user/instagram_profile_data_update";
        public static string IGProfileDetailsAPI(string Username)
            => $"https://www.instagram.com/api/v1/users/web_profile_info/?username={Username}";
        public static string LicenseFile => GetHomeDirectory().CombinePath("License.txt");
        public static string API => GetHomeDirectory().CombinePath("API.txt");
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
