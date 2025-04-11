using System.Collections.ObjectModel;
using FeedFetcher.Interfaces;
using FeedFetcher.Models;
using FeedFetcher.Utilities;

namespace FeedFetcher.ViewModel
{
    public class LoggerViewModel : BindableBase, ILogger
    {
        private ObservableCollection<LogModel>
            _logs = new ObservableCollection<LogModel>();
        public ObservableCollection<LogModel> Logs
        {
            get => _logs;
            set=>SetProperty(ref _logs, value,nameof(Logs));
        }
        public void Log(string message)
        {
            try
            {
                Logs.Add(new LogModel
                {
                    Message = message,
                    DateTime = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt")
                });
                if (Logs.Count > 500)
                    Logs.RemoveAt(Logs.Count - 1);
            }
            catch { }
        }
    }
}
