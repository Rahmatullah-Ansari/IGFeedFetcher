using FeedFetcher.Utilities;

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
    public class SessionModel:BindableBase
    {
        private int index;
        public int Index
        {
            get => index;
            set=>SetProperty(ref index, value,nameof(Index));
        }
        private List<cookies> _cookies;
        public List<cookies> cookies
        {
            get => _cookies;
            set=>SetProperty(ref _cookies, value,nameof(cookies));
        }
        private string _cookieString;
        public string CookieString
        {
            get => _cookieString;
            set=>SetProperty(ref _cookieString, value,nameof(CookieString));
        }
        private string _status;
        public string Status
        {
            get => _status;
            set=>SetProperty(ref _status, value,nameof(Status));
        }
        private string _username="N/A";
        public string Username
        {
            get=> _username;
            set=>SetProperty(ref _username, value,nameof(Username));
        }
        private string _password;
        public string Password
        {
            get => _password;
            set=>SetProperty(ref _password, value,nameof(Password));
        }
    }
}
