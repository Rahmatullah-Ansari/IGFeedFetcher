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
        private ObservableCollection<FeedModels> _feeds=new ObservableCollection<FeedModels>();
        public CancellationTokenSource tokenSource = new CancellationTokenSource();
        private string _text = "Start";
        public string ButtonText
        {
            get => _text;
            set=>SetProperty(ref _text, value,nameof(ButtonText));
        }
        public ObservableCollection<FeedModels> FeedCollections
        {
            get { return _feeds; }
            set=>SetProperty(ref _feeds, value,nameof(FeedCollections));
        }
        private ObservableCollection<SessionModel> _sessions = new ObservableCollection<SessionModel>();
        private SessionModel _session = new SessionModel();
        public SessionModel Session
        {
            get { return _session; }
            set => SetProperty(ref _session, value,nameof(Session));
        }
        private string _cookies;
        public string CookiesString
        {
            get => _cookies;
            set => SetProperty(ref _cookies, value, nameof(CookiesString));
        }
        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value, nameof(Username));
        }
        private string _pass;
        public string Password
        {
            get => _pass;
            set => SetProperty(ref _pass, value, nameof(Password));
        }
        private string _api=FileUtility.GetAPI();
        public string API
        {
            get => _api;
            set => SetProperty(ref _api, value, nameof(API));
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
        public ICommand SaveProfileAPI { get; set; }
        public ICommand StartFetching { get; set; }
        #endregion

        #region Constructor

        public MainViewModel()
        {
            AddSession = new BaseCommand<object>(AddSessionExecute);
            SaveProfileAPI = new BaseCommand<object>(SaveProfileAPIExecute);
            StartFetching = new BaseCommand<object>(StartFetchExecute);
            DeleteSession = new BaseCommand<object>(DeleteSessionExecute);
            CopySession = new BaseCommand<object>(CopySessionSessionExecute);
            Sessions = [.. FileUtility.GetSavedSession()];

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

        private void SaveProfileAPIExecute(object obj)
        {
            try
            {
                if (!string.IsNullOrEmpty(API))
                    FileUtility.SaveAPI(API);
            }
            catch { }
        }
        private void StartFetchExecute(object obj)
        {
            try
            {
                if(Sessions.Count == 0)
                {
                    MessageBox.Show("Please add atleast one instagram account or session");
                    return;
                }
                if(ButtonText == "Stop")
                {
                    tokenSource.Cancel();
                    ButtonText = "Start";
                    return;
                }
                else
                {

                    ButtonText = "Stop";
                }
                var profile = obj as TextBox;
                if (!string.IsNullOrEmpty(profile.Text))
                {
                    var profileData = profile.Text;
                    profile.Text = string.Empty;
                    httpHelper.SetSession(Sessions.GetRandomItem()?.CookieString);
                    var profileUrl = GetProfileUrl(profileData);
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
