using MintPlayerCrawler.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MintPlayerCrawler.Data.Repositories
{
    internal class LinkRepository : ILinkRepository
    {
        private MintPlayerCrawlerContext mintplayer_crawler_context;
        public LinkRepository(MintPlayerCrawlerContext mintplayer_crawler_context)
        {
            this.mintplayer_crawler_context = mintplayer_crawler_context;
        }

        public IEnumerable<Dtos.Link> GetLinks()
        {
            var links = mintplayer_crawler_context.Links
                .Select(link => ToDto(link, false));
            return links;
        }

        public IEnumerable<Dtos.Link> GetLinks(int count, int page)
        {
            var links = mintplayer_crawler_context.Links
                .Skip((page - 1) * count)
                .Take(count)
                .Select(link => ToDto(link, false));
            return links;
        }

        public Dtos.Link GetLink(int id)
        {
            var link = mintplayer_crawler_context.Links
                .SingleOrDefault(l => l.Id == id);
            return ToDto(link);
        }

        public async Task<Dtos.Link> InsertLink(Dtos.Link link)
        {
            var link_entity = ToEntity(link, mintplayer_crawler_context);
            mintplayer_crawler_context.Links.Add(link_entity);

            await mintplayer_crawler_context.SaveChangesAsync();

            return ToDto(link_entity);
        }

        public async Task<Dtos.Link> UpdateLink(Dtos.Link link)
        {
            var link_entity = mintplayer_crawler_context.Links
                .SingleOrDefault(l => l.Id == link.Id);

            link_entity.Url = link.Url;

            return ToDto(link_entity);
        }

        public async Task SaveChangesAsync()
        {
            await mintplayer_crawler_context.SaveChangesAsync();
        }

        #region Conversion methods
        internal static Dtos.Link ToDto(Entities.Link link, bool include_relations = false)
        {
            if (link == null) return null;

            if (include_relations)
            {
                return new Dtos.Link
                {
                    Id = link.Id,
                    Url = link.Url,
                    Via = ToDto(link.Via)
                };
            }
            else
            {
                return new Dtos.Link
                {
                    Id = link.Id,
                    Url = link.Url
                };
            }
        }
        internal static Entities.Link ToEntity(Dtos.Link link, MintPlayerCrawlerContext mintplayer_crawler_context)
        {
            if (link == null) return null;

            return new Entities.Link
            {
                Id = link.Id,
                Url = link.Url,
                Via = link.Via == null ? null : mintplayer_crawler_context.Links.Find(link.Via.Id)
            };
        }
        #endregion
    }
}
