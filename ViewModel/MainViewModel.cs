using FeedFetcher.Models;
using FeedFetcher.Utilities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FeedFetcher.ViewModel
{
    public class MainViewModel:BindableBase
    {
        private static MainViewModel instance;
        public static MainViewModel Instance => instance ?? (instance = new MainViewModel());
        private static readonly JsonJArrayHandler handler = JsonJArrayHandler.GetInstance;
        private ObservableCollection<SessionModel> _sessions = new ObservableCollection<SessionModel>();
        private SessionModel _session = new SessionModel();
        public SessionModel Session
        {
            get { return _session; }
            set => SetProperty(ref _session, value,nameof(Session));
        }
        public ObservableCollection<SessionModel> Sessions
        {
            get => _sessions;
            set =>SetProperty(ref _sessions, value,nameof(Sessions));
        }
        private HttpHelper httpHelper = HttpHelper.Instance;
        #region ICommand
        public ICommand AddSession { get; set; }
        public ICommand DeleteSession { get; set; }
        public ICommand CopySession { get; set; }
        public ICommand GetProfileDetails { get; set; }
        #endregion

        #region Constructor

        public MainViewModel()
        {
            AddSession = new BaseCommand<object>(AddSessionExecute);
            GetProfileDetails = new BaseCommand<object>(GetProfileDetailsExecute);
            DeleteSession = new BaseCommand<object>(DeleteSessionExecute);
            CopySession = new BaseCommand<object>(CopySessionSessionExecute);
            Sessions = new ObservableCollection<SessionModel>(FileUtility.GetSavedSession());

        }

        private void CopySessionSessionExecute(object obj)
        {
            try
            {
                if (Session != null)
                {
                    Clipboard.SetText(Session?.CookieString);
                }
            }
            catch { }
        }

        private void DeleteSessionExecute(object obj)
        {
            try
            {
                if (Session != null)
                {
                    Sessions.Remove(Session);
                    FileUtility.SaveSession(handler.Serialize(Sessions));
                }
            }
            catch { }
        }

        private void GetProfileDetailsExecute(object obj)
        {
            try
            {
                var profile = obj as TextBox;
                if (!string.IsNullOrEmpty(profile.Text))
                {
                    var profileData = profile.Text;
                    profile.Text = string.Empty;
                    httpHelper.SetSession(Sessions.GetRandomItem()?.CookieString);
                    var profileUrl = GetProfileUrl(profileData);
                    var feedResponse1 = httpHelper.GetFeedResponse(profileUrl);
                    var loginResponse = httpHelper.MOBILELogin();
                    if (loginResponse.IsMobileLoggedIn)
                    {
                        var feedResponse = httpHelper.GetFeedResponse(profileUrl);
                    }
                }
            }
            catch { }
        }

        private string GetProfileUrl(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text) && !text.Contains(".instagram.com"))
                    return $"https://www.instagram.com/{text}/";
                return text;
            }
            catch { return text; }
        }

        private void AddSessionExecute(object obj)
        {
            try
            {
                var session = obj as TextBox;
                if (!string.IsNullOrEmpty(session.Text))
                {
                    var model = new SessionModel
                    {
                        Index = Sessions.Count + 1,
                        CookieString = session.Text,
                        cookies = handler.Deserialize<List<cookies>>(session.Text)
                    };
                    Sessions.Add(model);
                    FileUtility.SaveSession(handler.Serialize(Sessions));
                    session.Text = string.Empty;
                }
            }
            catch { }
        }

        #endregion
    }
}
