using MintPlayer.Fetcher.Dtos;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher
{
    public interface IParser
    {
        Task<bool> IsMatch(string html);
        Task<Subject> Parse(string html, bool trimTrash);
    }
}
