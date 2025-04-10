using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFetcher.Utilities;

namespace FeedFetcher.Response
{
    public class ProfileIDResponseHandler
    {
        public PaginationModel model {  get; set; }=new PaginationModel();
        private JsonJArrayHandler handler => JsonJArrayHandler.GetInstance;
        public List<string> ListIDS { get; set; }=new List<string>();
        public ProfileIDResponseHandler(string Response)
        {
            try
            {
                if (string.IsNullOrEmpty(Response))
                    return;
                var obj = handler.ParseJsonToJObject(Response);
                model.NextPageUrl = handler.GetJTokenValue(obj, "next_page_url");
                int.TryParse(handler.GetJTokenValue(obj, "last_page"), out int last_page);
                int.TryParse(handler.GetJTokenValue(obj, "current_page"), out int current_page);
                int.TryParse(handler.GetJTokenValue(obj, "total"), out int total);
                model.CurrentPage = current_page;
                model.LastPage = last_page;
                model.TotalPages = total;
                if (model.LastPage == model.CurrentPage)
                    model = new PaginationModel();
                var ids = handler.GetJArrayElement(handler.GetJTokenValue(obj, "data"));
                if(ids != null && ids.HasValues)
                {
                    foreach(var id in ids)
                    {
                        var name = handler.GetJTokenValue(id, "insta_username");
                        if (!string.IsNullOrEmpty(name) && !ListIDS.Any(x => x == name))
                            ListIDS.Add(name);
                    }
                }
            }
            catch { }
        }
    }
}
