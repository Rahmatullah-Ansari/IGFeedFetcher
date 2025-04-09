using FeedFetcher.Utilities;

namespace FeedFetcher.Models
{
    public class InstagramAccount : BindableBase
    {
        private int _id;
        private string _name;
        private string _email;
        private string _password;
        private Proxy _proxy = new Proxy();
        private string _proxyString;
        private string _emailPass;
        private string _status = AccountStatus.NotChecked.ToString();
        private string _cookies;
        public int ID
        {
            get => _id;
            set => SetProperty(ref _id, value, nameof(ID));
        }
        public string Username
        {
            get => _name;
            set => SetProperty(ref _name, value, nameof(Username));
        }
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value, nameof(Password));
        }
        public Proxy Proxy
        {
            get => _proxy;
            set => SetProperty(ref _proxy, value, nameof(Proxy));
        }
        public string ProxyString
        {
            get => _proxyString;
            set => SetProperty(ref _proxyString, value, nameof(ProxyString));
        }
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value, nameof(Email));
        }
        public string EmailPass
        {
            get => _emailPass;
            set => SetProperty(ref _emailPass, value, nameof(EmailPass));
        }
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value, nameof(Status));
        }
        public string Cookies
        {
            get => _cookies;
            set => SetProperty(ref _cookies, value, nameof(Cookies));
        }
    }
    public enum AccountStatus
    {
        NotChecked,
        TryingToLogin,
        Suspended,
        TwoFactorLogin,
        Success,
        InvalidCredential,
        CaptchaVerification,
        AccountCompromised,
        ResetPassword,
        ProxyNotWorking,
        SoftBlocked,
        ChallengeRequired,
        Busy,
        Free
    }
    public class Proxy
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ProxyIp { get; set; }
        public int ProxyPort { get; set; }
        public bool HaveCred { get; set; }
    }
}
