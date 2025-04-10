namespace FeedFetcher.Utilities
{
    public class PaginationModel
    {
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int LastPage {  get; set; }
        public string NextPageUrl { get; set; } = IGConstants.UserIdAPI;
        public bool HasMoreResults { get; set; } = true;
    }
}
