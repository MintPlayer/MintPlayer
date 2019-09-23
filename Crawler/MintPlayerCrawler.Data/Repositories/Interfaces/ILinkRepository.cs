using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MintPlayerCrawler.Data.Repositories.Interfaces
{
    public interface ILinkRepository
    {
        IEnumerable<Dtos.Link> GetLinks();
        IEnumerable<Dtos.Link> GetLinks(int count, int page);
        Dtos.Link GetLink(int id);
        Task<Dtos.Link> InsertLink(Dtos.Link link);
        Task<Dtos.Link> UpdateLink(Dtos.Link link);
        Task SaveChangesAsync();
    }
}
