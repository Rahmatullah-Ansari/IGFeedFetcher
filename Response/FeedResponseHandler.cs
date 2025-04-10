using System.Collections.ObjectModel;
using FeedFetcher.Models;
using FeedFetcher.Utilities;

namespace FeedFetcher.Response
{
    public class FeedResponseHandler
    {
        public ObservableCollection<FeedModels>
            FeedsCollection = new ObservableCollection<FeedModels>();
        private JsonJArrayHandler handler => JsonJArrayHandler.GetInstance;
        public string JsonResponse {  get; set; }
        public FeedResponseHandler(string Response="",bool IsMobile=false)
        {
            try
            {
                if (IsMobile)
                {
                    Response = BuildMobileResponse(Response);
                }
                JsonResponse = Response;
            }
            catch { }
        }

        private string BuildMobileResponse(string response)
        {

            return response;
        }
    }
}
