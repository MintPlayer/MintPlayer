using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MintPlayer.AspNetCore.SpaServices.Prerendering.Services;
using MintPlayer.AspNetCore.SpaServices.Routing;
using MintPlayer.Pagination;
using MintPlayer.Web.Extensions;

namespace MintPlayer.Web.Services
{
	public class SpaRouteService : ISpaPrerenderingService
	{
		#region Constructor
		private readonly ISpaRouteService spaRouteService;
		public SpaRouteService(ISpaRouteService spaRouteService)
		{
			this.spaRouteService = spaRouteService;
		}
		#endregion

		public Task BuildRoutes(ISpaRouteBuilder routeBuilder)
		{
			routeBuilder
				.Route("", "home")
				.Group("search", "search", search_routes => search_routes
					.Route("", "search")
					.Route("{searchterm}", "results")
				)
				.Group("account", "account", account_routes => account_routes
					.Route("login", "login")
					.Route("register", "register")
					.Route("profile", "profile")
					.Group("password/reset", "passwordreset", password_routes => password_routes
						.Route("request", "request")
						.Route("perform", "perform")
					)
				)
				.Group("person", "person", person_routes => person_routes
					.Route("", "list")
					.Route("create", "create")
					.Route("favorite", "favorite")
					.Route("{id}", "show")
					.Route("{id}/{name}", "show-name")
					.Route("{id}/edit", "edit")
					.Route("{id}/{name}/edit", "edit-name")
				)
				.Group("artist", "artist", artist_routes => artist_routes
					.Route("", "list")
					.Route("create", "create")
					.Route("favorite", "favorite")
					.Route("{id}", "show")
					.Route("{id}/{name}", "show-name")
					.Route("{id}/edit", "edit")
					.Route("{id}/{name}/edit", "edit-name")
				)
				.Group("song", "song", song_routes => song_routes
					.Route("", "list")
					.Route("create", "create")
					.Route("favorite", "favorite")
					.Route("{id}", "show")
					.Route("{id}/{title}", "show-title")
					.Route("{id}/edit", "edit")
					.Route("{id}/{title}/edit", "edit-title")
				)
				.Group("playlist", "playlist", playlist_routes => playlist_routes
					.Route("", "list")
					.Route("create", "create")
					.Route("{id}", "show")
					.Route("{id}/{description}", "show-description")
					.Route("{id}/edit", "edit")
					.Route("{id}/{description}/edit", "edit-description")
				)
				.Group("mediumtype", "mediumtype", mediumtype_routes => mediumtype_routes
					.Route("", "list")
					.Route("create", "create")
					.Route("{id}", "show")
					.Route("{id}/{name}", "show-name")
					.Route("{id}/edit", "edit")
					.Route("{id}/{name}/edit", "edit-name")
				)
				.Group("tag", "tag", tag_routes => tag_routes
					.Group("category", "category", category_routes => category_routes
						.Route("", "list")
						.Route("create", "create")
						.Route("{categoryid}", "show")
						.Route("{categoryid}/edit", "edit")

						.Group("{categoryid}/tags", "tags", category_tag_routes => category_tag_routes
							.Route("create", "create")
							.Route("{tagid}", "show")
							.Route("{tagid}/edit", "edit")
						)
					)
				)
				.Group("community", "community", community_routes => community_routes
					.Group("blog", "blog", blog_routes => blog_routes
						.Route("", "list")
						.Route("create", "create")
						.Route("{blogpostid}/{blogpost_title}", "show")
						.Route("{blogpostid}/{blogpost_title}/edit", "edit")
					)
				)
				.Group("gdpr", "gdpr", gdpr_routes => gdpr_routes
					.Route("privacy-policy", "privacypolicy")
					.Route("terms-of-service", "termsofservice")
				);
			return Task.CompletedTask;
		}

		public async Task OnSupplyData(HttpContext context, IDictionary<string, object> data)
		{
			var route = await spaRouteService.GetCurrentRoute(context);
			var personService = context.RequestServices.GetService<Data.Abstractions.Services.IPersonService>();
			var artistService = context.RequestServices.GetService<Data.Abstractions.Services.IArtistService>();
			var songService = context.RequestServices.GetService<Data.Abstractions.Services.ISongService>();
			var mediumTypeService = context.RequestServices.GetService<Data.Abstractions.Services.IMediumTypeService>();
			var accountService = context.RequestServices.GetService<Data.Services.IAccountService>();
			var tagCategoryService = context.RequestServices.GetService<Data.Abstractions.Services.ITagCategoryService>();
			var tagService = context.RequestServices.GetService<Data.Abstractions.Services.ITagService>();
			var playlistService = context.RequestServices.GetService<Data.Abstractions.Services.IPlaylistService>();
			var blogPostService = context.RequestServices.GetService<Data.Abstractions.Services.IBlogPostService>();

			var user = await accountService.GetCurrentUser(context.User);
			data["user"] = user;

			switch (route?.Name)
			{
				case null:
					{
						context.Response.OnStarting(() =>
						{
							context.Response.StatusCode = 404;
							return Task.CompletedTask;
						});
					}
					break;
				case "person-list":
					{
						var req = new PaginationRequest<Dtos.Dtos.Person> { PerPage = 20, Page = 1, SortProperty = "FirstName", SortDirection = System.ComponentModel.ListSortDirection.Ascending };
						var people = await personService.PagePeople(req);
						data["people"] = people;
					}
					break;
				case "person-create":
					{
						var mediumtypes = await mediumTypeService.GetMediumTypes(false);
						data["mediumtypes"] = mediumtypes.ToArray();
					}
					break;
				case "person-show-name":
				case "person-edit":
					{
						var id = Convert.ToInt32(route.Parameters["id"]);
						var person = await personService.GetPerson(id, true);
						if (person == null)
						{
							context.Response.OnStarting(() =>
							{
								context.Response.StatusCode = 404;
								return Task.CompletedTask;
							});
						}
						else
						{
							data["person"] = person;
							context.Response.OnStarting(() =>
							{
								context.Response.Headers["last-modified"] = person.DateUpdate.ToISOString();
								return Task.CompletedTask;
							});
						}
						var mediumtypes = await mediumTypeService.GetMediumTypes(false);
						data["mediumtypes"] = mediumtypes.ToArray();
					}
					break;
				case "artist-list":
					{
						var req = new PaginationRequest<Dtos.Dtos.Artist> { PerPage = 20, Page = 1, SortProperty = "Name", SortDirection = System.ComponentModel.ListSortDirection.Ascending };
						var artists = await artistService.PageArtists(req);
						data["artists"] = artists;
					}
					break;
				case "artist-create":
					{
						var mediumtypes = await mediumTypeService.GetMediumTypes(false);
						data["mediumtypes"] = mediumtypes.ToArray();
					}
					break;
				case "artist-show-name":
				case "artist-edit":
					{
						var id = Convert.ToInt32(route.Parameters["id"]);
						var artist = await artistService.GetArtist(id, true);
						if (artist == null)
						{
							context.Response.OnStarting(() =>
							{
								context.Response.StatusCode = 404;
								return Task.CompletedTask;
							});
						}
						else
						{
							data["artist"] = artist;
							context.Response.OnStarting(() =>
							{
								context.Response.Headers["last-modified"] = artist.DateUpdate.ToISOString();
								return Task.CompletedTask;
							});
						}
						var mediumtypes = await mediumTypeService.GetMediumTypes(false);
						data["mediumtypes"] = mediumtypes.ToArray();
					}
					break;
				case "song-list":
					{
						var req = new PaginationRequest<Dtos.Dtos.Song> { PerPage = 20, Page = 1, SortProperty = "Title", SortDirection = System.ComponentModel.ListSortDirection.Ascending };
						var songs = await songService.PageSongs(req);
						data["songs"] = songs;
					}
					break;
				case "song-create":
					{
						var mediumtypes = await mediumTypeService.GetMediumTypes(false);
						data["mediumtypes"] = mediumtypes.ToArray();
					}
					break;
				case "song-show-title":
				case "song-edit":
					{
						var id = Convert.ToInt32(route.Parameters["id"]);
						var song = await songService.GetSong(id, true);
						if (song == null)
						{
							context.Response.OnStarting(() =>
							{
								context.Response.StatusCode = 404;
								return Task.CompletedTask;
							});
						}
						else
						{
							data["song"] = song;
							context.Response.OnStarting(() =>
							{
								context.Response.Headers["last-modified"] = song.DateUpdate.ToISOString();
								return Task.CompletedTask;
							});
						}
						var mediumtypes = await mediumTypeService.GetMediumTypes(false);
						data["mediumtypes"] = mediumtypes.ToArray();
					}
					break;
				case "mediumtype-list":
					{
						var mediumtypes = await mediumTypeService.GetMediumTypes(false);
						data["mediumtypes"] = mediumtypes.ToArray();
					}
					break;
				case "mediumtype-create":
					break;
				case "mediumtype-show":
				case "mediumtype-edit":
					{
						var id = Convert.ToInt32(route.Parameters["id"]);
						var mediumtype = await mediumTypeService.GetMediumType(id, false);
						if (mediumtype == null)
						{
							context.Response.OnStarting(() =>
							{
								context.Response.StatusCode = 404;
								return Task.CompletedTask;
							});
						}
						else
						{
							data["mediumtype"] = mediumtype;
						}
					}
					break;
				case "account-profile":
					{
						var logins = await accountService.GetExternalLogins(context.User);
						var providers = await accountService.GetProviders();
						data["logins"] = logins;
						data["providers"] = providers;
					}
					break;
				case "tag-category-list":
					{
						var categories = await tagCategoryService.GetTagCategories();
						data["tagcategories"] = categories.ToArray();
					}
					break;
				case "tag-category-show":
					{
						var id = Convert.ToInt32(route.Parameters["categoryid"]);
						var category = await tagCategoryService.GetTagCategory(id, true);
						if (category == null)
							context.Response.OnStarting(() =>
							{
								context.Response.StatusCode = 404;
								return Task.CompletedTask;
							});
						else
							data["tagcategory"] = category;
					}
					break;
				case "tag-category-edit":
				case "tag-category-tags-create":
					{
						var id = Convert.ToInt32(route.Parameters["categoryid"]);
						var category = await tagCategoryService.GetTagCategory(id);
						if (category == null)
							context.Response.OnStarting(() =>
							{
								context.Response.StatusCode = 404;
								return Task.CompletedTask;
							});
						else
							data["tagcategory"] = category;
					}
					break;
				case "tag-category-tags-show":
					{
						var tagId = Convert.ToInt32(route.Parameters["tagid"]);
						var tag = await tagService.GetTag(tagId, true);
						if (tag == null)
							context.Response.OnStarting(() =>
							{
								context.Response.StatusCode = 404;
								return Task.CompletedTask;
							});
						else
							data["tag"] = tag;
					}
					break;
				case "tag-category-tags-edit":
					{
						var tagId = Convert.ToInt32(route.Parameters["tagid"]);
						var tag = await tagService.GetTag(tagId, false);
						if (tag == null)
							context.Response.OnStarting(() =>
							{
								context.Response.StatusCode = 404;
								return Task.CompletedTask;
							});
						else
							data["tag"] = tag;
					}
					break;
				case "community-blog-list":
					{
						var blogposts = await blogPostService.GetBlogPosts();
						data["blogposts"] = blogposts.ToArray();
					}
					break;
				case "community-blog-show":
				case "community-blog-edit":
					{
						var blogPostId = Convert.ToInt32(route.Parameters["blogpostid"]);
						var blogPost = await blogPostService.GetBlogPost(blogPostId);
						if (blogPost == null)
							context.Response.OnStarting(() =>
							{
								context.Response.StatusCode = 404;
								return Task.CompletedTask;
							});
						else
							data["blogpost"] = blogPost;

						//data["blogpost"] = new Data.Dtos.Blog.BlogPost
						//{
						//    Id = 2,
						//    Title = "Test",
						//    Headline = "This is a test",
						//    Body = "This is a test",
						//    Author = new Dtos.Dtos.User
						//    {
						//        Id = Guid.Empty,
						//        Email = "pieterjandeclippel@msn.com",
						//        UserName = "PieterjanDC",
						//        PictureUrl = "https://mintplayer.com/profile/5"
						//    },
						//    Published = DateTime.Now
						//};
					}
					break;
				//case "playlist-list":
				//    {
				//        var req = new Pagination.PaginationRequest<Data.Dtos.Playlist> { PerPage = 20, Page = 1, SortProperty = "Description", SortDirection = System.ComponentModel.ListSortDirection.Ascending };
				//        var playlists = playlistService.PagePlaylists(req);
				//        data["playlists"] = playlists;
				//    }
				//    break;
				//case "playlist-show":
				//case "playlist-edit":
				//    {
				//        var id = System.Convert.ToInt32(route.Parameters["id"]);
				//        var playlist = playlistService.GetPlaylist(id, true);
				//        if (playlist == null)
				//        {
				//            context.Response.OnStarting(() =>
				//            {
				//                context.Response.StatusCode = 404;
				//                return Task.CompletedTask;
				//            });
				//        }
				//        else
				//        {
				//            data["playlist"] = playlist;
				//        }
				//    }
				//    break;
				default:
					break;
			}
		}
	}
}
