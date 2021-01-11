using System.Threading.Tasks;

namespace MintPlayerCrawler.Data.Repositories.Interfaces
{
    public interface IRequestJobRepository
    {
        Task<Dtos.Jobs.RequestJob> CreateRequestJob(Dtos.Jobs.RequestJob job);
    }
}
