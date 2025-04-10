using FeedFetcher.Models;
using System.IO;

namespace FeedFetcher.Utilities
{
    public static class FileUtility
    {
        private static JsonJArrayHandler handler => JsonJArrayHandler.GetInstance;
        public static bool SaveSession(string v)
        {
            try
            {
                File.WriteAllText(IGConstants.SessionFileName, v);
                return true;
            }
            catch { return false;}
        }
        public static bool SaveAPI(string api)
        {
            try
            {
                File.WriteAllText(IGConstants.API, api);
                return true;
            }
            catch { return false; }
        }
        public static string GetAPI()
        {
            try
            {
                return File.ReadAllText(IGConstants.API);
            }
            catch
            {
                return IGConstants.UserIdAPI;
            }
        }
        public static IEnumerable<SessionModel> GetSavedSession()
        {
            try
            {
                return handler.Deserialize<List<SessionModel>>(File.ReadAllText(IGConstants.SessionFileName));
            }
            catch {return new List<SessionModel>(); }
        }
    }
}
