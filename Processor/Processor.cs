using FeedFetcher.Interfaces;
using FeedFetcher.IOCAndServices;
using FeedFetcher.Models;
using FeedFetcher.Utilities;
using FeedFetcher.ViewModel;

namespace FeedFetcher.Processor
{
    public class Processor
    {
        public static Processor Instance=>new Processor();
        private HttpHelper httpHelper = HttpHelper.Instance;
        private ILogger logger { get; set; }
        public Processor()
        {
            logger = InstanceProvider.GetInstance<ILogger>();
        }
        private SessionModel session {  get; set; }
        public async Task Start(string Profileid,CancellationToken token, MainViewModel mainViewModel)
        {
            try
            {
                var feedResponse = await httpHelper.GetFeedResponse(Profileid);
                if (feedResponse is null || (!feedResponse.HasFeed && !feedResponse.NotFound))
                    feedResponse = await httpHelper.GetFeedResponse(Profileid, true);
                if(feedResponse != null && feedResponse.HasFeed)
                {
                    await PostFeed(Profileid, feedResponse?.JsonResponse);
                    token.ThrowIfCancellationRequested();
                    ThreadFactory.Instance.Start(() =>
                    {
                        foreach (var data in feedResponse?.FeedsCollection)
                        {
                            token.ThrowIfCancellationRequested();
                            MainViewModel.Instance.FeedCollections.Add(data);
                        }
                    });
                    token.ThrowIfCancellationRequested();
                }
                else
                {
                    logger.Log($"{Profileid} Not Found Or Don't Have Feed");
                }
            }
            catch (OperationCanceledException)
            {
                throw new OperationCanceledException();
            }
        }

        private async Task PostFeed(string? id, string? jsonResponse)
        {
            var postedResponse =  await httpHelper.PostAsync(IGConstants.PostAPI, GetPostBody(id,jsonResponse));
        }

        private string? GetPostBody(string? id, string? FinalJsonResponse)
        {
            return $"{{\"data\":{FinalJsonResponse},\"username\":\"{id}\"}}"; ;
        }

        public void Init(SessionModel sessionModel)
        {
            this.session = sessionModel;
            httpHelper.SetSession(sessionModel);
        }
    }
}
