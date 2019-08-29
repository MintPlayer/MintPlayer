using System;
using System.Collections.Generic;
using SitemapXml.Interfaces;

namespace SitemapXml.DependencyInjection.Interfaces
{
    public interface ISitemapXml
    {
        /// <summary>Computes a <code>list</code> of sitemap urls (paging)</summary>
        /// <typeparam name="T">Type of data you want to display in a sitemap</typeparam>
        /// <param name="items">List of all items to display in the sitemap-index</param>
        /// <param name="perPage">Number of items in one sitemap</param>
        /// <param name="urlFunc">Function to compute the url</param>
        IEnumerable<Sitemap> GetSitemapIndex<T>(IEnumerable<T> items, int perPage, Func<int, int, string> urlFunc) where T : ITimestamps;
    }
}
