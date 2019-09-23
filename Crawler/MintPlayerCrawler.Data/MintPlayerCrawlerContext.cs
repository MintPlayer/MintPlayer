using Microsoft.EntityFrameworkCore;
using MintPlayerCrawler.Data.Entities;

namespace MintPlayerCrawler.Data
{
    internal class MintPlayerCrawlerContext : DbContext
    {
        internal DbSet<Link> Links { get; set; }

        public MintPlayerCrawlerContext() : base()
        {
        }

        public MintPlayerCrawlerContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            var connection_string = @"Server=(localdb)\mssqllocaldb;Database=MintPlayerCrawler;Trusted_Connection=True;ConnectRetryCount=0";
            optionsBuilder.UseSqlServer(connection_string);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
