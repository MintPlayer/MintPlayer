using System;
using System.Collections.Generic;
using SitemapXml.Interfaces;

namespace MintPlayer.Data.Dtos
{

    public class Subject : ITimestamps
    {
        public int Id { get; set; }

        public virtual string Text { get; }
        public List<Medium> Media { get; set; }

        [Nest.Ignore]
        public DateTime DateUpdate { get; set; }
    }
}
