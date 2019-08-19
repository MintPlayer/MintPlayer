using System.Threading.Tasks;
using System.Collections.Generic;

namespace MintPlayer.Data.Repositories.Interfaces
{
    public interface ISongRepository
    {
        IEnumerable<Dtos.Song> GetSongs(bool include_relations = false);
        IEnumerable<Dtos.Song> GetSongs(int count, int page, bool include_relations = false);
        Dtos.Song GetSong(int id, bool include_relations = false);
        Task<Dtos.Song> InsertSong(Dtos.Song song);
        Task<Dtos.Song> UpdateSong(Dtos.Song song);
        Task DeleteSong(int song_id);
        Task SaveChangesAsync();
    }
}
