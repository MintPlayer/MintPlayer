﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MintPlayer.Data;

namespace MintPlayer.Data.Migrations
{
    [DbContext(typeof(MintPlayerContext))]
    partial class MintPlayerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<int>("RoleId");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<int>("UserId");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<int>", b =>
                {
                    b.Property<int>("UserId");

                    b.Property<int>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.Property<int>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.ArtistPerson", b =>
                {
                    b.Property<int>("ArtistId");

                    b.Property<int>("PersonId");

                    b.Property<bool>("Active");

                    b.HasKey("ArtistId", "PersonId");

                    b.HasIndex("PersonId");

                    b.ToTable("ArtistPerson");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.ArtistSong", b =>
                {
                    b.Property<int>("ArtistId");

                    b.Property<int>("SongId");

                    b.HasKey("ArtistId", "SongId");

                    b.HasIndex("SongId");

                    b.ToTable("ArtistSong");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Lyrics", b =>
                {
                    b.Property<int>("SongId");

                    b.Property<int>("UserId");

                    b.Property<string>("Text");

                    b.Property<DateTime>("UpdatedAt");

                    b.HasKey("SongId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("Lyrics");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Subject", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("SubjectType")
                        .IsRequired();

                    b.Property<int?>("UserDeleteId");

                    b.Property<int?>("UserInsertId");

                    b.Property<int?>("UserUpdateId");

                    b.HasKey("Id");

                    b.HasIndex("UserDeleteId");

                    b.HasIndex("UserInsertId");

                    b.HasIndex("UserUpdateId");

                    b.ToTable("Subject");

                    b.HasDiscriminator<string>("SubjectType").HasValue("Subject");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("PictureUrl");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
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

            modelBuilder.Entity("MintPlayer.Data.Entities.Artist", b =>
                {
                    b.HasBaseType("MintPlayer.Data.Entities.Subject");

                    b.Property<string>("Name");

                    b.Property<int?>("YearQuit");

                    b.Property<int?>("YearStarted");

                    b.ToTable("Subjects");

                    b.HasDiscriminator().HasValue("artist");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Person", b =>
                {
                    b.HasBaseType("MintPlayer.Data.Entities.Subject");

                    b.Property<DateTime?>("Born");

                    b.Property<DateTime?>("Died");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.ToTable("Subjects");

                    b.HasDiscriminator().HasValue("person");
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Song", b =>
                {
                    b.HasBaseType("MintPlayer.Data.Entities.Subject");

                    b.Property<DateTime>("Released");

                    b.Property<string>("Title");

                    b.ToTable("Subjects");

                    b.HasDiscriminator().HasValue("song");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<int>", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("MintPlayer.Data.Entities.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.ArtistPerson", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.Artist", "Artist")
                        .WithMany("Members")
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("MintPlayer.Data.Entities.Person", "Person")
                        .WithMany("Artists")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.ArtistSong", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.Artist", "Artist")
                        .WithMany("Songs")
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("MintPlayer.Data.Entities.Song", "Song")
                        .WithMany("Artists")
                        .HasForeignKey("SongId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("MintPlayer.Data.Entities.Lyrics", b =>
                {
                    b.HasOne("MintPlayer.Data.Entities.Song", "Song")
                        .WithMany("Lyrics")
                        .HasForeignKey("SongId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("MintPlayer.Data.Entities.User", "User")
                        .WithMany("Lyrics")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict);
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
#pragma warning restore 612, 618
        }
    }
}
