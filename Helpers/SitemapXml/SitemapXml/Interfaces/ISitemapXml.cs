using System;
using System.Collections.Generic;
using SitemapXml.Interfaces;

namespace SitemapXml.SitemapXml.Interfaces
{
    public interface ISitemapXml
    {
        IEnumerable<Url> GetSitemapIndex<T>(IEnumerable<T> items, int perPage, Func<int, int, string> urlFunc) where T : ITimestamps;
    }
}
