namespace MintPlayer.Fetcher.SongtekstenNet.Data
{
    internal class Song
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }

        public MintPlayer.Fetcher.Abstractions.Dtos.Song ToDto()
        {
            return new MintPlayer.Fetcher.Abstractions.Dtos.Song
            {
                Id = Id,
                Title = Title,
                Url = Url
            };
        }
    }
}
