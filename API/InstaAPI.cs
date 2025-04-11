using FeedFetcher.Models;
using FeedFetcher.Utilities;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using System.Net;
using System.Net.Http;

namespace FeedFetcher.API
{
    public class InstaAPI
    {
        public static InstaAPI Instance(InstagramAccount account) => new InstaAPI(account);
        private InstagramAccount account { get; set; }
        public IInstaApi instaApi { get; set; }
        public InstaAPI(InstagramAccount instagramAccount)
        {
            this.account = instagramAccount;
        }
        public async Task<InstaAPI> Build()
        {
            var api = InstaApiBuilder.CreateBuilder();
            if (!string.IsNullOrEmpty(account.Proxy.ProxyIp) && account.Proxy.ProxyPort > 0)
            {
                var proxy = new WebProxy
                {
                    Address = new Uri($"http://{account.Proxy.ProxyIp?.Trim()}:{account.Proxy.ProxyPort}"),
                    BypassProxyOnLocal = true,
                    UseDefaultCredentials = false
                };
                if (!string.IsNullOrEmpty(account.Proxy.Username) && !string.IsNullOrEmpty(account.Proxy.Password))
                {
                    proxy.Credentials = new NetworkCredential(account.Proxy.Username, account.Proxy.Password);
                }
                var httpClientHandler = new HttpClientHandler { Proxy = proxy, UseProxy = true };
                api.UseHttpClientHandler(httpClientHandler);
            }
            instaApi = api.Build();
            if (!string.IsNullOrEmpty(account.Cookies))
            {
                await instaApi.LoadStateDataFromStringAsync(account.Cookies);
            }
            else
            {
                instaApi.SetUser(account.Username, account.Password);
            }

            if (!instaApi.IsUserAuthenticated)
            {
                var login = await instaApi.LoginAsync();
                if (instaApi.IsUserAuthenticated)
                {
                    var cookies = await instaApi.GetStateDataAsStringAsync();
                    account.Cookies = cookies;
                }
            }
            return this;
        }
    }
}
