using FeedFetcher.Utilities;

namespace FeedFetcher.Models
{
    public class LogModel:BindableBase
    {
        private string dateTime;
        public string DateTime
        {
            get => dateTime;
            set=>SetProperty(ref dateTime, value,nameof(DateTime));
        }
        private string message=string.Empty;
        public string Message
        {
            get => message;
            set=>SetProperty(ref message, value,nameof(Message));
        }
    }
}
