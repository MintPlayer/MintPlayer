using MintPlayer.Dtos.Dtos;
using System;

namespace MintPlayer.Data.Abstractions.Dtos.Blog
{
    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Headline { get; set; }
        public string Body { get; set; }
        public User Author { get; set; }
        public DateTime Published { get; set; }
    }
}
