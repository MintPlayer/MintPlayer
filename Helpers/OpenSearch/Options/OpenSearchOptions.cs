namespace OpenSearch.Options
{
    public class OpenSearchOptions
    {
        /// <summary>URL where the OpenSearch Description is served</summary>
        public string OsdxEndpoint { get; set; }

        /// <summary>URL where the user is redirected for the search</summary>
        public string SearchUrl { get; set; }
        
        /// <summary>URL to provide suggestions</summary>
        public string SuggestUrl { get; set; }

        /// <summary>URL where the image is located</summary>
        public string ImageUrl { get; set; }
    }
}
