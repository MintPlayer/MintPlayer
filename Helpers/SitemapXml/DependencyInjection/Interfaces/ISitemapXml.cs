using System;
using System.Collections.Generic;
using SitemapXml.Interfaces;

namespace SitemapXml.DependencyInjection.Interfaces
{
    public interface ISitemapXml
    {
        IEnumerable<Sitemap> GetSitemapIndex<T>(IEnumerable<T> items, int perPage, Func<int, int, string> urlFunc) where T : ITimestamps;
    }
}
