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

        /// <summary>Short name of your search engine</summary>
        public string ShortName { get; set; }

        /// <summary>Description for your search engine</summary>
        public string Description { get; set; }

        /// <summary>Email to contact when there is an issue with the search engine</summary>
        public string Contact { get; set; }
    }
}
