using System.IO;
using System.Text;

namespace FeedFetcher.Utilities
{
    internal static class SU
    {
        public static bool vld(byte[] k, bool act = false)
        {
            var txt = Encoding.UTF8.GetString(k);
            var tt = Convert.FromBase64String(txt.Replace("IGF-", ""));
            return vldk(Encoding.UTF8.GetString(tt), act);
        }

        private static bool vldk(string txt, bool act = false)
        {
            try
            {
                var t = gTextCap(txt, act);
                var s = DateTime.Now.Ticks.ToString();
                long.TryParse(s, out long lll);
                long.TryParse(t, out long ll);
                var isk = lll <= ll;
                if (isk)
                    sv($"IGF-{t}-$");
                MainWindow.wd?.lvd(new DateTime(ll).ToString("dd-MM-yyyy hh:mm:ss tt"));
                return isk;
            }
            catch { sv(txt); return false; }
        }

        private static string gTextCap(string txt, bool act)
        {
            var t = txt.Replace("IGF-", "").Replace("-$", "");
            if (act)
            {
                if (t.Contains("m"))
                {
                    int.TryParse(t.Replace("m", ""), out int tm);
                    t = DateTime.Now.AddMinutes(tm).Ticks.ToString();
                }
                else if (t.Contains("h"))
                {
                    int.TryParse(t.Replace("h", ""), out int tm);
                    t = DateTime.Now.AddHours(tm).Ticks.ToString();
                }
                else if (t.Contains("d"))
                {
                    int.TryParse(t.Replace("d", ""), out int tm);
                    t = DateTime.Now.AddDays(tm).Ticks.ToString();
                }
                else if (t.Contains("mt"))
                {
                    int.TryParse(t.Replace("mt", ""), out int tm);
                    t = DateTime.Now.AddMonths(tm).Ticks.ToString();
                }
                else if (t.Contains("y"))
                {
                    int.TryParse(t.Replace("y", ""), out int tm);
                    t = DateTime.Now.AddYears(tm).Ticks.ToString();
                }
                return t;
            }
            else
                return t;
        }

        public static void sv(string txt)
        {
            var bse = Convert.ToBase64String(Encoding.UTF8.GetBytes(txt));
            var tt = Encoding.UTF8.GetBytes(bse);
            File.WriteAllBytes(IGConstants.LicenseFile, tt);
        }
    }
}
