namespace FeedFetcher.Utilities
{
    public class Proxy : BindableBase
    {
        public Proxy()
        {
            this.HasCredentials = false;
            this.HasProxy = false;
        }

        public bool HasCredentials { get; private set; }

        public bool HasProxy { get; private set; }

        public string Ip
        {
            get; set;
        }

        public int Port
        {
            get; set;
        }

        public string Password
        {
            get; set;
        }

        public string Username
        {
            get; set;
        }


    }
}
