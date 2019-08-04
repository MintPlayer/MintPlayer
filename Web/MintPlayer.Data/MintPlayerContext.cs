using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MintPlayer.Data.Entities;

namespace MintPlayer.Data
{
    internal class MintPlayerContext : IdentityDbContext<User, Role, int>
    {
        internal DbSet<Person> People { get; set; }
        internal DbSet<Artist> Artists { get; set; }
        internal DbSet<Song> Songs { get; set; }

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

            modelBuilder.Entity<Person>().HasBaseType<Subject>().ToTable("Subjects");
            modelBuilder.Entity<Artist>().HasBaseType<Subject>().ToTable("Subjects");
            modelBuilder.Entity<Song>().HasBaseType<Subject>().ToTable("Subjects");

            // Many-to-many Artist-Person
            modelBuilder.Entity<ArtistPerson>().HasKey(ap => new { ap.ArtistId, ap.PersonId });
            modelBuilder.Entity<ArtistPerson>().HasOne(ap => ap.Artist).WithMany(a => a.Members).HasForeignKey(ap => ap.ArtistId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ArtistPerson>().HasOne(ap => ap.Person).WithMany(p => p.Artists).HasForeignKey(ap => ap.PersonId).OnDelete(DeleteBehavior.Restrict);

            // Many-to-many Artist-Song
            modelBuilder.Entity<ArtistSong>().HasKey(@as => new { @as.ArtistId, @as.SongId });
            modelBuilder.Entity<ArtistSong>().HasOne(@as => @as.Artist).WithMany(a => a.Songs).HasForeignKey(@as => @as.ArtistId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ArtistSong>().HasOne(@as => @as.Song).WithMany(s => s.Artists).HasForeignKey(@as => @as.SongId).OnDelete(DeleteBehavior.Restrict);

            // Many-to-many Song-User (Lyrics)
            modelBuilder.Entity<Lyrics>().HasKey(lyric => new { lyric.SongId, lyric.UserId });
            modelBuilder.Entity<Lyrics>().HasOne(lyric => lyric.Song).WithMany(s => s.Lyrics).HasForeignKey(su => su.SongId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Lyrics>().HasOne(lyric => lyric.User).WithMany(u => u.Lyrics).HasForeignKey(su => su.UserId).OnDelete(DeleteBehavior.Restrict);

            // Discriminator Subject
            modelBuilder.Entity<Subject>().Property(s => s.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Subject>().HasQueryFilter(s => s.UserDelete == null);
            modelBuilder.Entity<Subject>().HasDiscriminator<string>("SubjectType")
                .HasValue<Person>("person")
                .HasValue<Artist>("artist")
                .HasValue<Song>("song");
        }
    }
}
