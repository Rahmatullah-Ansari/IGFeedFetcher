namespace FeedFetcher.Models
{
    public class cookies
    {
        public string domian {  get; set; }
        public string expirationDate {  get; set; }
        public bool hostOnly {  get; set; }
        public bool httpOnly {  get; set; }
        public string name {  get; set; }
        public string path {  get; set; }
        public string sameSite { get; set; }
        public bool secure {  get; set; }
        public bool session {  get; set; }
        public string storeId {  get; set; }
        public string value {  get; set; }
    }
    public class SessionModel
    {
        public int Index { get; set; }
        public List<cookies> cookies { get; set; }
        public string CookieString { get; set; }
        public string Status { get; set; }
    }
}
