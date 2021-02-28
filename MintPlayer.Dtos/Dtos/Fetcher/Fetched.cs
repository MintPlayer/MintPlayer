using MintPlayer.Dtos.Enums;

namespace MintPlayer.Dtos.Dtos.Fetcher
{
    public class Fetched<T>
    {
        public T Item { get; set; }
        public eFetchedCertainty Certainty { get; set; }
    }
}
