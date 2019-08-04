using MintPlayer.Data.Dtos;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MintPlayer.Data.Repositories.Interfaces
{
    public interface IMediumRepository
    {
        Task StoreMedia(Subject subject, IEnumerable<Medium> media);
    }
}
