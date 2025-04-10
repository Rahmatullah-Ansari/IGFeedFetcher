using FeedFetcher.Utilities;

namespace FeedFetcher.Models
{
    public class FeedModels:BindableBase
    {
        private List<MediaInfo> _mediaInfos=new List<MediaInfo>();
        public List<MediaInfo> Medias
        {
            get => _mediaInfos;
            set => SetProperty(ref _mediaInfos, value,nameof(Medias));
        }
        private string _username;
        public string Username
        {
            get => _username;
            set=>SetProperty(ref _username,value,nameof(Username));
        }
        private int _likeCount;
        public int LikeCount
        {
            get => _likeCount;
            set => SetProperty(ref _likeCount, value, nameof(LikeCount));
        }
        private int _CommentCount;
        public int CommentCount
        {
            get => _CommentCount;
            set => SetProperty(ref _CommentCount, value, nameof(CommentCount));
        }
        private int _ShareCount;
        public int ShareCount
        {
            get => _ShareCount;
            set => SetProperty(ref _ShareCount, value, nameof(ShareCount));
        }
    }
    public class MediaInfo: BindableBase
    {
        private string _mediaurl;
        public string MediaUrl
        {
            get { return _mediaurl; }
            set=>SetProperty(ref _mediaurl, value,nameof(MediaUrl));
        }
    }
}
