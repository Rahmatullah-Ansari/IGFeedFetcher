using FeedFetcher.Models;
using FeedFetcher.Utilities;

namespace FeedFetcher.Processor
{
    public class Processor
    {
        public static Processor Instance=>new Processor();
        private HttpHelper httpHelper = HttpHelper.Instance;
        private SessionModel session {  get; set; }
        public async Task Start(string Profileid,CancellationToken token)
        {
            try
            {
                var feedResponse = await httpHelper.GetFeedResponse(Profileid);
                if (feedResponse is null || !feedResponse.HasFeed)
                    feedResponse = await httpHelper.GetFeedResponse(Profileid, true);
                if(feedResponse != null && feedResponse.HasFeed)
                {
                    await PostFeed(Profileid, feedResponse?.JsonResponse);
                }
            }
            catch (OperationCanceledException)
            {
                throw new OperationCanceledException();
            }
        }

        private async Task PostFeed(string? id, string? jsonResponse)
        {
            await httpHelper.PostAsync(IGConstants.PostAPI, GetPostBody(id,jsonResponse));
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
