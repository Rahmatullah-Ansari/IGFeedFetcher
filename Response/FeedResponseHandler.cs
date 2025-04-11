using System.Collections.ObjectModel;
using FeedFetcher.Models;
using FeedFetcher.Utilities;

namespace FeedFetcher.Response
{
    public class FeedResponseHandler
    {
        public bool HasFeed { get; set; } = true;
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
                GetDetails(JsonResponse);
                HasFeed = !string.IsNullOrEmpty(JsonResponse)
                    && FeedsCollection.Count > 0;
            }
            catch { }
        }

        private void GetDetails(string jsonResponse)
        {
            try
            {
                var obj = handler.ParseJsonToJObject(jsonResponse);
                var medias = handler.GetJArrayElement(handler.GetJTokenValue(obj, "edge_owner_to_timeline_media", "edges"));
                if(medias != null && medias.HasValues)
                {
                    foreach(var media in medias)
                    {
                        int.TryParse(handler.GetJTokenValue(media, "node", "edge_liked_by", "count"), out int likes);
                        int.TryParse(handler.GetJTokenValue(media, "node", "edge_media_to_comment", "count"), out int comment);
                        var imagemedias = new List<MediaInfo>();
                        var thumnails = handler.GetJArrayElement(handler.GetJTokenValue(media, "thumbnail_resources"));
                        if(thumnails !=null && thumnails.HasValues)
                        {
                            foreach(var thumbnail in thumnails)
                            {
                                imagemedias.Add(new MediaInfo
                                {
                                    MediaUrl = handler.GetJTokenValue(thumbnail, "src")
                                });
                            }
                        }
                        var model = new FeedModels
                        {
                            Username = handler.GetJTokenValue(media,"node", "owner", "username"),
                            LikeCount = likes,
                            CommentCount = comment,
                            Medias = imagemedias
                        };
                        if (!FeedsCollection.Any(x => x.Username == model.Username))
                            FeedsCollection.Add(model);
                    }
                }
            }
            catch { }
        }

        private string BuildMobileResponse(string response)
        {

            return response;
        }
    }
}
