using Microsoft.EntityFrameworkCore;
using MintPlayerCrawler.Data.Entities;
using MintPlayerCrawler.Data.Entities.Jobs;

namespace MintPlayerCrawler.Data
{
    internal class MintPlayerCrawlerContext : DbContext
    {
        internal DbSet<Link> Links { get; set; }

        internal DbSet<Job> Jobs { get; set; }
        internal DbSet<RequestJob> RequestJobs { get; set; }
        internal DbSet<FetchUrlsJob> FetchUrlsJobs { get; set; }
        internal DbSet<IndexJob> IndexJobs { get; set; }

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

            modelBuilder.Entity<Job>().HasDiscriminator<string>("JobType")
                .HasValue<RequestJob>("request")
                .HasValue<FetchUrlsJob>("fetch_urls")
                .HasValue<IndexJob>("index");
        }
    }
}
