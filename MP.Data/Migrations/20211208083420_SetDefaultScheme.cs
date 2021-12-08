using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MintPlayer.Data.Migrations
{
    public partial class SetDefaultScheme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "mintplay");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "mintplay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "mintplay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PictureUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bypass2faForExternalLogin = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                schema: "mintplay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogEntries",
                schema: "mintplay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "mintplay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "mintplay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "mintplay",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "mintplay",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "mintplay",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlogPosts",
                schema: "mintplay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Headline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserInsertId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserUpdateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserDeleteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateDelete = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogPosts_AspNetUsers_UserDeleteId",
                        column: x => x.UserDeleteId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BlogPosts_AspNetUsers_UserInsertId",
                        column: x => x.UserInsertId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BlogPosts_AspNetUsers_UserUpdateId",
                        column: x => x.UserUpdateId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MediumTypes",
                schema: "mintplay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Visible = table.Column<bool>(type: "bit", nullable: false),
                    UserInsertId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserUpdateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserDeleteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediumTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediumTypes_AspNetUsers_UserDeleteId",
                        column: x => x.UserDeleteId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MediumTypes_AspNetUsers_UserInsertId",
                        column: x => x.UserInsertId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MediumTypes_AspNetUsers_UserUpdateId",
                        column: x => x.UserUpdateId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Playlists",
                schema: "mintplay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Accessibility = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playlists_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                schema: "mintplay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConcurrencyStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    UserInsertId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserUpdateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserDeleteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateDelete = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subjects_AspNetUsers_UserDeleteId",
                        column: x => x.UserDeleteId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Subjects_AspNetUsers_UserInsertId",
                        column: x => x.UserInsertId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Subjects_AspNetUsers_UserUpdateId",
                        column: x => x.UserUpdateId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TagCategories",
                schema: "mintplay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Color = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserInsertId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserUpdateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserDeleteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateDelete = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagCategories_AspNetUsers_UserDeleteId",
                        column: x => x.UserDeleteId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TagCategories_AspNetUsers_UserInsertId",
                        column: x => x.UserInsertId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TagCategories_AspNetUsers_UserUpdateId",
                        column: x => x.UserUpdateId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Artists",
                schema: "mintplay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YearStarted = table.Column<int>(type: "int", nullable: true),
                    YearQuit = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Artists_Subjects_Id",
                        column: x => x.Id,
                        principalSchema: "mintplay",
                        principalTable: "Subjects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ElasticSearchIndexJobs",
                schema: "mintplay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: true),
                    SubjectStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElasticSearchIndexJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElasticSearchIndexJobs_Jobs_Id",
                        column: x => x.Id,
                        principalSchema: "mintplay",
                        principalTable: "Jobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ElasticSearchIndexJobs_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalSchema: "mintplay",
                        principalTable: "Subjects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Likes",
                schema: "mintplay",
                columns: table => new
                {
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoesLike = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => new { x.SubjectId, x.UserId });
                    table.ForeignKey(
                        name: "FK_Likes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Likes_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalSchema: "mintplay",
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Media",
                schema: "mintplay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeId = table.Column<int>(type: "int", nullable: true),
                    SubjectId = table.Column<int>(type: "int", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Media_MediumTypes_TypeId",
                        column: x => x.TypeId,
                        principalSchema: "mintplay",
                        principalTable: "MediumTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Media_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalSchema: "mintplay",
                        principalTable: "Subjects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "People",
                schema: "mintplay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Born = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Died = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                    table.ForeignKey(
                        name: "FK_People_Subjects_Id",
                        column: x => x.Id,
                        principalSchema: "mintplay",
                        principalTable: "Subjects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Songs",
                schema: "mintplay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Released = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Songs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Songs_Subjects_Id",
                        column: x => x.Id,
                        principalSchema: "mintplay",
                        principalTable: "Subjects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                schema: "mintplay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    UserInsertId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserUpdateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserDeleteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateDelete = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_AspNetUsers_UserDeleteId",
                        column: x => x.UserDeleteId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tags_AspNetUsers_UserInsertId",
                        column: x => x.UserInsertId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tags_AspNetUsers_UserUpdateId",
                        column: x => x.UserUpdateId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tags_TagCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "mintplay",
                        principalTable: "TagCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tags_Tags_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "mintplay",
                        principalTable: "Tags",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ArtistPerson",
                schema: "mintplay",
                columns: table => new
                {
                    ArtistId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistPerson", x => new { x.ArtistId, x.PersonId });
                    table.ForeignKey(
                        name: "FK_ArtistPerson_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalSchema: "mintplay",
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArtistPerson_People_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "mintplay",
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArtistSong",
                schema: "mintplay",
                columns: table => new
                {
                    ArtistId = table.Column<int>(type: "int", nullable: false),
                    SongId = table.Column<int>(type: "int", nullable: false),
                    Credited = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistSong", x => new { x.ArtistId, x.SongId });
                    table.ForeignKey(
                        name: "FK_ArtistSong_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalSchema: "mintplay",
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArtistSong_Songs_SongId",
                        column: x => x.SongId,
                        principalSchema: "mintplay",
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lyrics",
                schema: "mintplay",
                columns: table => new
                {
                    SongId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timeline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lyrics", x => new { x.SongId, x.UserId });
                    table.ForeignKey(
                        name: "FK_Lyrics_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "mintplay",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lyrics_Songs_SongId",
                        column: x => x.SongId,
                        principalSchema: "mintplay",
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistSong",
                schema: "mintplay",
                columns: table => new
                {
                    PlaylistId = table.Column<int>(type: "int", nullable: false),
                    SongId = table.Column<int>(type: "int", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistSong", x => new { x.PlaylistId, x.SongId, x.Index });
                    table.ForeignKey(
                        name: "FK_PlaylistSong_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalSchema: "mintplay",
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlaylistSong_Songs_SongId",
                        column: x => x.SongId,
                        principalSchema: "mintplay",
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubjectTag",
                schema: "mintplay",
                columns: table => new
                {
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectTag", x => new { x.SubjectId, x.TagId });
                    table.ForeignKey(
                        name: "FK_SubjectTag_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalSchema: "mintplay",
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectTag_Tags_TagId",
                        column: x => x.TagId,
                        principalSchema: "mintplay",
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "mintplay",
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { new Guid("91f3cec8-a67d-45f3-b718-22cf71961b05"), null, "Administrator", "Administrator" });

            migrationBuilder.InsertData(
                schema: "mintplay",
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { new Guid("93c9bda5-8254-486f-ade1-95b5b66e83db"), null, "Blogger", "Blogger" });

            migrationBuilder.CreateIndex(
                name: "IX_ArtistPerson_PersonId",
                schema: "mintplay",
                table: "ArtistPerson",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistSong_SongId",
                schema: "mintplay",
                table: "ArtistSong",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "mintplay",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "mintplay",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "mintplay",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "mintplay",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "mintplay",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "mintplay",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "mintplay",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_UserDeleteId",
                schema: "mintplay",
                table: "BlogPosts",
                column: "UserDeleteId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_UserInsertId",
                schema: "mintplay",
                table: "BlogPosts",
                column: "UserInsertId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_UserUpdateId",
                schema: "mintplay",
                table: "BlogPosts",
                column: "UserUpdateId");

            migrationBuilder.CreateIndex(
                name: "IX_ElasticSearchIndexJobs_SubjectId",
                schema: "mintplay",
                table: "ElasticSearchIndexJobs",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserId",
                schema: "mintplay",
                table: "Likes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Lyrics_UserId",
                schema: "mintplay",
                table: "Lyrics",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Media_SubjectId",
                schema: "mintplay",
                table: "Media",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Media_TypeId",
                schema: "mintplay",
                table: "Media",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MediumTypes_UserDeleteId",
                schema: "mintplay",
                table: "MediumTypes",
                column: "UserDeleteId");

            migrationBuilder.CreateIndex(
                name: "IX_MediumTypes_UserInsertId",
                schema: "mintplay",
                table: "MediumTypes",
                column: "UserInsertId");

            migrationBuilder.CreateIndex(
                name: "IX_MediumTypes_UserUpdateId",
                schema: "mintplay",
                table: "MediumTypes",
                column: "UserUpdateId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_UserId",
                schema: "mintplay",
                table: "Playlists",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistSong_SongId",
                schema: "mintplay",
                table: "PlaylistSong",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_UserDeleteId",
                schema: "mintplay",
                table: "Subjects",
                column: "UserDeleteId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_UserInsertId",
                schema: "mintplay",
                table: "Subjects",
                column: "UserInsertId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_UserUpdateId",
                schema: "mintplay",
                table: "Subjects",
                column: "UserUpdateId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectTag_TagId",
                schema: "mintplay",
                table: "SubjectTag",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_TagCategories_UserDeleteId",
                schema: "mintplay",
                table: "TagCategories",
                column: "UserDeleteId");

            migrationBuilder.CreateIndex(
                name: "IX_TagCategories_UserInsertId",
                schema: "mintplay",
                table: "TagCategories",
                column: "UserInsertId");

            migrationBuilder.CreateIndex(
                name: "IX_TagCategories_UserUpdateId",
                schema: "mintplay",
                table: "TagCategories",
                column: "UserUpdateId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_CategoryId",
                schema: "mintplay",
                table: "Tags",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_ParentId",
                schema: "mintplay",
                table: "Tags",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserDeleteId",
                schema: "mintplay",
                table: "Tags",
                column: "UserDeleteId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserInsertId",
                schema: "mintplay",
                table: "Tags",
                column: "UserInsertId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserUpdateId",
                schema: "mintplay",
                table: "Tags",
                column: "UserUpdateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtistPerson",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "ArtistSong",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "BlogPosts",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "ElasticSearchIndexJobs",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "Likes",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "LogEntries",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "Lyrics",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "Media",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "PlaylistSong",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "SubjectTag",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "People",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "Artists",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "Jobs",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "MediumTypes",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "Playlists",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "Songs",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "Tags",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "Subjects",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "TagCategories",
                schema: "mintplay");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "mintplay");
        }
    }
}
