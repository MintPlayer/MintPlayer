using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MintPlayer.Data.Entities;

namespace MintPlayer.Data
{
    internal class MintPlayerContext : IdentityDbContext<User, Role, int>
    {
        public MintPlayerContext() : base()
        {
        }

        public MintPlayerContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            var connection_string = @"Server=(localdb)\mssqllocaldb;Database=MintPlayer;Trusted_Connection=True;ConnectRetryCount=0";
            optionsBuilder.UseSqlServer(connection_string);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
