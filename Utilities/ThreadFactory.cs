namespace FeedFetcher.Utilities
{
    public class ThreadFactory
    {
        public static ThreadFactory Instance=>new ThreadFactory();
        public Thread Start(ThreadStart threadStart, string Name="",bool IsBackground = false,bool IsStart=true)
        {
            var thread = new Thread(threadStart);
            thread.IsBackground = IsBackground;
            if(!string.IsNullOrEmpty(Name)) thread.Name = Name;
            if(IsStart) thread.Start();
            return thread;
        }
    }
}
