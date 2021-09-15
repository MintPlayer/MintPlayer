using Microsoft.EntityFrameworkCore;
using MintPlayer.Crawler.Data.Entities;

namespace MintPlayer.Crawler.Data
{
    internal class MintPlayerCrawlerContext : DbContext
    {
        public MintPlayerCrawlerContext() : base()
        {
        }

        const string ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=MintPlayerCrawler;Trusted_Connection=True;ConnectRetryCount=0";
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Song> Songs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(ConnectionString);
        }
    }
}
