using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MintPlayer.Data.Entities;
using MintPlayer.Data.Entities.Blog;
using MintPlayer.Data.Entities.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MintPlayer.Data
{
	internal class MintPlayerContext : IdentityDbContext<User, Role, Guid>
	{
		// dotnet ef migrations add AddIdentity
		// dotnet ef database update

		// To generate production migrations
		// dotnet ef migrations script --output "MigrationScripts\AddIdentity.sql" --context MintPlayerContext
		// dotnet ef migrations script AddIdentity --output "MigrationScripts\AddCoreEntities.sql" --context MintPlayerContext
		//
		// dotnet ef migrations add AddLikes
		// dotnet ef migrations script AddCoreEntities --output "MigrationScripts\AddLikes.sql" --context MintPlayerContext
		
		// dotnet ef migrations script AddTimestamps --output "MigrationScripts\AddMediumTypeVisible.sql" --context MintPlayerContext
		internal DbSet<Person> People { get; set; }
		internal DbSet<Artist> Artists { get; set; }
		internal DbSet<Song> Songs { get; set; }
		internal DbSet<Like> Likes { get; set; }
		internal DbSet<MediumType> MediumTypes { get; set; }
		internal DbSet<Medium> Media { get; set; }
		internal DbSet<Subject> Subjects { get; set; }
		internal DbSet<Entities.Jobs.Job> Jobs { get; set; }
		internal DbSet<Entities.Jobs.ElasticSearchIndexJob> ElasticSearchIndexJobs { get; set; }
		internal DbSet<TagCategory> TagCategories { get; set; }
		internal DbSet<Tag> Tags { get; set; }
		internal DbSet<Playlist> Playlists { get; set; }
		internal DbSet<BlogPost> BlogPosts { get; set; }
		internal DbSet<LogEntry> LogEntries { get; set; }

		private readonly IConfiguration configuration;
		public MintPlayerContext(IConfiguration configuration)
		{
			this.configuration = configuration;
		}
		public MintPlayerContext()
		{
			this.configuration = null;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);
			if (configuration == null)
			{
				// Only used when generating migrations
				var migrationsConnectionString = @"Server=(localdb)\mssqllocaldb;Database=MintPlayer;Trusted_Connection=True;ConnectRetryCount=0";
				optionsBuilder.UseSqlServer(migrationsConnectionString, options => options.MigrationsAssembly("MintPlayer.Data"));
			}
			else
			{
				optionsBuilder.UseSqlServer(configuration.GetConnectionString("MintPlayer"));
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<IdentityUserLogin<Guid>>()
				.Property(ut => ut.LoginProvider)
				.HasMaxLength(50);
			modelBuilder.Entity<IdentityUserLogin<Guid>>()
				.Property(ut => ut.ProviderKey)
				.HasMaxLength(200);

			modelBuilder.Entity<IdentityUserToken<Guid>>()
				.Property(ut => ut.LoginProvider)
				.HasMaxLength(50);
			modelBuilder.Entity<IdentityUserToken<Guid>>()
				.Property(ut => ut.Name)
				.HasMaxLength(50);

			// Get role names
			var roles = GetRoles();
			modelBuilder.Entity<Role>().HasData(roles);

			// Subjects
			modelBuilder.Entity<Subject>().ToTable("Subjects");

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
			modelBuilder.Entity<Lyrics>().Property(lyric => lyric.Timeline).HasConversion(
				i => JsonConvert.SerializeObject(i.Select(v => Convert.ToInt32(v * 20)).ToArray()),
				o => o == null ? new System.Collections.Generic.List<double>() : string.IsNullOrEmpty(o) ? new System.Collections.Generic.List<double>() : JsonConvert.DeserializeObject<int[]>(o).Select(v => (v / 20.0d)).ToList()
			);

			// Discriminator Subject
			modelBuilder.Entity<Subject>().Property(s => s.Id).ValueGeneratedOnAdd();
			modelBuilder.Entity<Subject>().HasQueryFilter(s => s.UserDelete == null);
			modelBuilder.Entity<Subject>().HasDiscriminator<string>("SubjectType")
				.HasValue<Person>("person")
				.HasValue<Artist>("artist")
				.HasValue<Song>("song");

			// Artist.Credited default true
			modelBuilder.Entity<ArtistSong>().Property(@as => @as.Credited).HasDefaultValue(true);

			// Many-to-many Subject-User (Like)
			modelBuilder.Entity<Like>().HasKey(like => new { like.SubjectId, like.UserId });
			modelBuilder.Entity<Like>().HasOne(like => like.Subject).WithMany(s => s.Likes).HasForeignKey(su => su.SubjectId).OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<Like>().HasOne(like => like.User).WithMany(u => u.Likes).HasForeignKey(su => su.UserId).OnDelete(DeleteBehavior.Restrict);

			// QueryFilters Tags
			modelBuilder.Entity<TagCategory>()
				.HasQueryFilter(tc => tc.UserDelete == null)
				.Property(tc => tc.Color)
					.HasConversion(color => color.ToArgb(), argb => System.Drawing.Color.FromArgb(argb));
			modelBuilder.Entity<Tag>().HasQueryFilter(s => s.UserDelete == null);

			// Many-to-many Subject-Tag
			modelBuilder.Entity<SubjectTag>().HasKey(st => new { st.SubjectId, st.TagId });
			modelBuilder.Entity<SubjectTag>().HasOne(st => st.Subject).WithMany(s => s.Tags).HasForeignKey(st => st.SubjectId).OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<SubjectTag>().HasOne(st => st.Tag).WithMany(t => t.Subjects).HasForeignKey(st => st.TagId).OnDelete(DeleteBehavior.Restrict);

			// Medium types
			modelBuilder.Entity<MediumType>().HasQueryFilter(mt => mt.UserDelete == null);

			// Jobs
			modelBuilder.Entity<Entities.Jobs.Job>().ToTable("Jobs");

			// Discriminator Job
			modelBuilder.Entity<Entities.Jobs.Job>().Property(j => j.Id).ValueGeneratedOnAdd();
			modelBuilder.Entity<Entities.Jobs.Job>().HasDiscriminator<string>("JobType")
				.HasValue<Entities.Jobs.ElasticSearchIndexJob>("elasticsearch");

			// Playlist
			modelBuilder.Entity<Playlist>().HasQueryFilter(p => !p.IsDeleted);
			modelBuilder.Entity<PlaylistSong>().HasKey(ps => new { ps.PlaylistId, ps.SongId, ps.Index });
			modelBuilder.Entity<PlaylistSong>().HasOne(ps => ps.Playlist).WithMany(playlist => playlist.Tracks).HasForeignKey(ps => ps.PlaylistId).OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<PlaylistSong>().HasOne(ps => ps.Song).WithMany(song => song.Tracks).HasForeignKey(ps => ps.SongId).OnDelete(DeleteBehavior.Restrict);

			// BlogPost
			modelBuilder.Entity<BlogPost>().HasQueryFilter(bp => bp.UserDelete == null);
		}

		/// <summary>Reads the configured application roles from the configuration.</summary>
		/// <param name="configuration">Application configuration</param>
		private IEnumerable<Role> GetRoles()
		{
			var roles = new List<MintPlayer.Dtos.Dtos.Role>
			{
				new MintPlayer.Dtos.Dtos.Role
				{
					Id = new Guid("93c9bda5-8254-486f-ade1-95b5b66e83db"),
					Name = "Blogger"
				},
				new MintPlayer.Dtos.Dtos.Role
				{
					Id = new Guid("91f3cec8-a67d-45f3-b718-22cf71961b05"),
					Name = "Administrator"
				}
			};
			return roles.Select(r => new Role
			{
				Id = r.Id,
				Name = r.Name,
				NormalizedName = r.Name.Normalize(),
				ConcurrencyStamp = null
			});
		}
	}
}
