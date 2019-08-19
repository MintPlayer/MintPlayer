using System.Threading.Tasks;
using System.Collections.Generic;

namespace MintPlayer.Data.Repositories.Interfaces
{
    public interface IArtistRepository
    {
        IEnumerable<Dtos.Artist> GetArtists(bool include_relations = false);
        IEnumerable<Dtos.Artist> GetArtists(int count, int page, bool include_relations = false);
        Dtos.Artist GetArtist(int id, bool include_relations = false);
        Task<Dtos.Artist> InsertArtist(Dtos.Artist artist);
        Task<Dtos.Artist> UpdateArtist(Dtos.Artist artist);
        Task DeleteArtist(int artist_id);
        Task SaveChangesAsync();
    }
}
