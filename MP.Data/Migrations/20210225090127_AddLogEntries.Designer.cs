﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MintPlayer.Data;

namespace MintPlayer.Data.Migrations
{
    [DbContext(typeof(MintPlayerContext))]
    [Migration("20210225090127_AddLogEntries")]
    partial class AddLogEntries
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50);

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50);

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50);

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.ArtistPerson", b =>
                {
                    b.Property<int>("ArtistId")
                        .HasColumnType("int");

                    b.Property<int>("PersonId")
                        .HasColumnType("int");

                    b.Property<bool>("Active")
                        .HasColumnType("bit");

                    b.HasKey("ArtistId", "PersonId");

                    b.HasIndex("PersonId");

                    b.ToTable("ArtistPerson");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.ArtistSong", b =>
                {
                    b.Property<int>("ArtistId")
                        .HasColumnType("int");

                    b.Property<int>("SongId")
                        .HasColumnType("int");

                    b.HasKey("ArtistId", "SongId");

                    b.HasIndex("SongId");

                    b.ToTable("ArtistSong");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Blog.BlogPost", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Body")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("DateDelete")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateInsert")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateUpdate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Headline")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("UserDeleteId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("UserInsertId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("UserUpdateId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserDeleteId");

                    b.HasIndex("UserInsertId");

                    b.HasIndex("UserUpdateId");

                    b.ToTable("BlogPosts");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Jobs.Job", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("JobType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Jobs");

                    b.HasDiscriminator<string>("JobType").HasValue("Job");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Like", b =>
                {
                    b.Property<int>("SubjectId")
                        .HasColumnType("int");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("DoesLike")
                        .HasColumnType("bit");

                    b.HasKey("SubjectId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("Likes");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Logging.LogEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("LogEntries");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Lyrics", b =>
                {
                    b.Property<int>("SongId")
                        .HasColumnType("int");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Timeline")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("SongId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("Lyrics");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Medium", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("SubjectId")
                        .HasColumnType("int");

                    b.Property<int?>("TypeId")
                        .HasColumnType("int");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SubjectId");

                    b.HasIndex("TypeId");

                    b.ToTable("Media");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.MediumType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PlayerType")
                        .HasColumnType("int");

                    b.Property<Guid?>("UserDeleteId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("UserInsertId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("UserUpdateId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Visible")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("UserDeleteId");

                    b.HasIndex("UserInsertId");

                    b.HasIndex("UserUpdateId");

                    b.ToTable("MediumTypes");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Playlist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Accessibility")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Playlists");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.PlaylistSong", b =>
                {
                    b.Property<int>("PlaylistId")
                        .HasColumnType("int");

                    b.Property<int>("SongId")
                        .HasColumnType("int");

                    b.Property<int>("Index")
                        .HasColumnType("int");

                    b.HasKey("PlaylistId", "SongId", "Index");

                    b.HasIndex("SongId");

                    b.ToTable("PlaylistSong");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Role", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");

                    b.HasData(
                        new
                        {
                            Id = new Guid("93c9bda5-8254-486f-ade1-95b5b66e83db"),
                            Name = "Blogger",
                            NormalizedName = "Blogger"
                        },
                        new
                        {
                            Id = new Guid("91f3cec8-a67d-45f3-b718-22cf71961b05"),
                            Name = "Administrator",
                            NormalizedName = "Administrator"
                        });
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Subject", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("DateDelete")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateInsert")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateUpdate")
                        .HasColumnType("datetime2");

                    b.Property<string>("SubjectType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("UserDeleteId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("UserInsertId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("UserUpdateId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserDeleteId");

                    b.HasIndex("UserInsertId");

                    b.HasIndex("UserUpdateId");

                    b.ToTable("Subjects");

                    b.HasDiscriminator<string>("SubjectType").HasValue("Subject");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.SubjectTag", b =>
                {
                    b.Property<int>("SubjectId")
                        .HasColumnType("int");

                    b.Property<int>("TagId")
                        .HasColumnType("int");

                    b.HasKey("SubjectId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("SubjectTag");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("CategoryId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DateDelete")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateInsert")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateUpdate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ParentId")
                        .HasColumnType("int");

                    b.Property<Guid?>("UserDeleteId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("UserInsertId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("UserUpdateId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("ParentId");

                    b.HasIndex("UserDeleteId");

                    b.HasIndex("UserInsertId");

                    b.HasIndex("UserUpdateId");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.TagCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Color")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DateDelete")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateInsert")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateUpdate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("UserDeleteId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("UserInsertId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("UserUpdateId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserDeleteId");

                    b.HasIndex("UserInsertId");

                    b.HasIndex("UserUpdateId");

                    b.ToTable("TagCategories");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("PictureUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Jobs.ElasticSearchIndexJob", b =>
                {
                    b.HasBaseType("MintPlayer.Data.Entities.Jobs.Job");

                    b.Property<int?>("SubjectId")
                        .HasColumnType("int");

                    b.Property<int>("SubjectStatus")
                        .HasColumnType("int");

                    b.HasIndex("SubjectId");

                    b.HasDiscriminator().HasValue("elasticsearch");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Artist", b =>
                {
                    b.HasBaseType("MintPlayer.Data.Entities.Subject");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("YearQuit")
                        .HasColumnType("int");

                    b.Property<int?>("YearStarted")
                        .HasColumnType("int");

                    b.HasDiscriminator().HasValue("artist");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Person", b =>
                {
                    b.HasBaseType("MintPlayer.Data.Entities.Subject");

                    b.Property<DateTime?>("Born")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Died")
                        .HasColumnType("datetime2");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.HasDiscriminator().HasValue("person");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Song", b =>
                {
                    b.HasBaseType("MintPlayer.Data.Entities.Subject");

                    b.Property<DateTime>("Released")
                        .HasColumnType("datetime2");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasDiscriminator().HasValue("song");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MintPlayer.Data.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.ArtistPerson", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.Artist", "Artist")
                        .WithMany("Members")
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MintPlayer.Data.Entities.Person", "Person")
                        .WithMany("Artists")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.ArtistSong", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.Artist", "Artist")
                        .WithMany("Songs")
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MintPlayer.Data.Entities.Song", "Song")
                        .WithMany("Artists")
                        .HasForeignKey("SongId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Blog.BlogPost", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.User", "UserDelete")
                        .WithMany()
                        .HasForeignKey("UserDeleteId");

                    b.HasOne("MintPlayer.Data.Entities.User", "UserInsert")
                        .WithMany()
                        .HasForeignKey("UserInsertId");

                    b.HasOne("MintPlayer.Data.Entities.User", "UserUpdate")
                        .WithMany()
                        .HasForeignKey("UserUpdateId");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Like", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.Subject", "Subject")
                        .WithMany("Likes")
                        .HasForeignKey("SubjectId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MintPlayer.Data.Entities.User", "User")
                        .WithMany("Likes")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Lyrics", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.Song", "Song")
                        .WithMany("Lyrics")
                        .HasForeignKey("SongId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MintPlayer.Data.Entities.User", "User")
                        .WithMany("Lyrics")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Medium", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.Subject", "Subject")
                        .WithMany("Media")
                        .HasForeignKey("SubjectId");

                    b.HasOne("MintPlayer.Data.Entities.MediumType", "Type")
                        .WithMany()
                        .HasForeignKey("TypeId");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.MediumType", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.User", "UserDelete")
                        .WithMany()
                        .HasForeignKey("UserDeleteId");

                    b.HasOne("MintPlayer.Data.Entities.User", "UserInsert")
                        .WithMany()
                        .HasForeignKey("UserInsertId");

                    b.HasOne("MintPlayer.Data.Entities.User", "UserUpdate")
                        .WithMany()
                        .HasForeignKey("UserUpdateId");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Playlist", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.PlaylistSong", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.Playlist", "Playlist")
                        .WithMany("Tracks")
                        .HasForeignKey("PlaylistId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MintPlayer.Data.Entities.Song", "Song")
                        .WithMany("Tracks")
                        .HasForeignKey("SongId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Subject", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.User", "UserDelete")
                        .WithMany()
                        .HasForeignKey("UserDeleteId");

                    b.HasOne("MintPlayer.Data.Entities.User", "UserInsert")
                        .WithMany()
                        .HasForeignKey("UserInsertId");

                    b.HasOne("MintPlayer.Data.Entities.User", "UserUpdate")
                        .WithMany()
                        .HasForeignKey("UserUpdateId");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.SubjectTag", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.Subject", "Subject")
                        .WithMany("Tags")
                        .HasForeignKey("SubjectId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MintPlayer.Data.Entities.Tag", "Tag")
                        .WithMany("Subjects")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Tag", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.TagCategory", "Category")
                        .WithMany("Tags")
                        .HasForeignKey("CategoryId");

                    b.HasOne("MintPlayer.Data.Entities.Tag", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentId");

                    b.HasOne("MintPlayer.Data.Entities.User", "UserDelete")
                        .WithMany()
                        .HasForeignKey("UserDeleteId");

                    b.HasOne("MintPlayer.Data.Entities.User", "UserInsert")
                        .WithMany()
                        .HasForeignKey("UserInsertId");

                    b.HasOne("MintPlayer.Data.Entities.User", "UserUpdate")
                        .WithMany()
                        .HasForeignKey("UserUpdateId");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.TagCategory", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.User", "UserDelete")
                        .WithMany()
                        .HasForeignKey("UserDeleteId");

                    b.HasOne("MintPlayer.Data.Entities.User", "UserInsert")
                        .WithMany()
                        .HasForeignKey("UserInsertId");

                    b.HasOne("MintPlayer.Data.Entities.User", "UserUpdate")
                        .WithMany()
                        .HasForeignKey("UserUpdateId");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Jobs.ElasticSearchIndexJob", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.Subject", "Subject")
                        .WithMany()
                        .HasForeignKey("SubjectId");
                });
#pragma warning restore 612, 618
        }
    }
}
