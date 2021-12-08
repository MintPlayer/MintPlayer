using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MintPlayer.AspNetCore.SpaServices.Routing;
using MintPlayer.Pagination;
using MintPlayer.Web.Extensions;

namespace MintPlayer.Web.Services
{
	public class SpaRouteService : ISpaPrerenderingService
	{
		private readonly ISpaRouteService spaRouteService;
		public SpaRouteService(ISpaRouteService spaRouteService)
		{
			this.spaRouteService = spaRouteService;
		}

		public Task OnSupplyData(HttpContext context, IDictionary<string, object> data)
		{
			var route = spaRouteService.GetCurrentRoute(context);
			var personService = context.RequestServices.GetService<Data.Abstractions.Services.IPersonService>();
			var artistService = context.RequestServices.GetService<Data.Abstractions.Services.IArtistService>();
			var songService = context.RequestServices.GetService<Data.Abstractions.Services.ISongService>();
			var mediumTypeService = context.RequestServices.GetService<Data.Abstractions.Services.IMediumTypeService>();
			var accountService = context.RequestServices.GetService<Data.Services.IAccountService>();
			var tagCategoryService = context.RequestServices.GetService<Data.Abstractions.Services.ITagCategoryService>();
			var tagService = context.RequestServices.GetService<Data.Abstractions.Services.ITagService>();
			var playlistService = context.RequestServices.GetService<Data.Abstractions.Services.IPlaylistService>();
			var blogPostService = context.RequestServices.GetService<Data.Abstractions.Services.IBlogPostService>();

			var user = accountService.GetCurrentUser(context.User).Result;
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
						var people = personService.PagePeople(req).Result;
						data["people"] = people;
					}
					break;
				case "person-create":
					{
						var mediumtypes = mediumTypeService.GetMediumTypes(false).Result;
						data["mediumtypes"] = mediumtypes.ToArray();
					}
					break;
				case "person-show-name":
				case "person-edit":
					{
						var id = Convert.ToInt32(route.Parameters["id"]);
						var person = personService.GetPerson(id, true).Result;
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
						var mediumtypes = mediumTypeService.GetMediumTypes(false).Result;
						data["mediumtypes"] = mediumtypes.ToArray();
					}
					break;
				case "artist-list":
					{
						var req = new PaginationRequest<Dtos.Dtos.Artist> { PerPage = 20, Page = 1, SortProperty = "Name", SortDirection = System.ComponentModel.ListSortDirection.Ascending };
						var artists = artistService.PageArtists(req).Result;
						data["artists"] = artists;
					}
					break;
				case "artist-create":
					{
						var mediumtypes = mediumTypeService.GetMediumTypes(false).Result;
						data["mediumtypes"] = mediumtypes.ToArray();
					}
					break;
				case "artist-show-name":
				case "artist-edit":
					{
						var id = Convert.ToInt32(route.Parameters["id"]);
						var artist = artistService.GetArtist(id, true).Result;
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
						var mediumtypes = mediumTypeService.GetMediumTypes(false).Result;
						data["mediumtypes"] = mediumtypes.ToArray();
					}
					break;
				case "song-list":
					{
						var req = new PaginationRequest<Dtos.Dtos.Song> { PerPage = 20, Page = 1, SortProperty = "Title", SortDirection = System.ComponentModel.ListSortDirection.Ascending };
						var songs = songService.PageSongs(req).Result;
						data["songs"] = songs;
					}
					break;
				case "song-create":
					{
						var mediumtypes = mediumTypeService.GetMediumTypes(false).Result;
						data["mediumtypes"] = mediumtypes.ToArray();
					}
					break;
				case "song-show-title":
				case "song-edit":
					{
						var id = Convert.ToInt32(route.Parameters["id"]);
						var song = songService.GetSong(id, true).Result;
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
						var mediumtypes = mediumTypeService.GetMediumTypes(false).Result;
						data["mediumtypes"] = mediumtypes.ToArray();
					}
					break;
				case "mediumtype-list":
					{
						var mediumtypes = mediumTypeService.GetMediumTypes(false).Result;
						data["mediumtypes"] = mediumtypes.ToArray();
					}
					break;
				case "mediumtype-create":
					break;
				case "mediumtype-show":
				case "mediumtype-edit":
					{
						var id = Convert.ToInt32(route.Parameters["id"]);
						var mediumtype = mediumTypeService.GetMediumType(id, false).Result;
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
						var logins = accountService.GetExternalLogins(context.User).Result;
						var providers = accountService.GetProviders().Result;
						data["logins"] = logins;
						data["providers"] = providers;
					}
					break;
				case "tag-category-list":
					{
						var categories = tagCategoryService.GetTagCategories().Result;
						data["tagcategories"] = categories.ToArray();
					}
					break;
				case "tag-category-show":
					{
						var id = Convert.ToInt32(route.Parameters["categoryid"]);
						var category = tagCategoryService.GetTagCategory(id, true).Result;
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
						var category = tagCategoryService.GetTagCategory(id).Result;
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
						var tag = tagService.GetTag(tagId, true).Result;
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
						var tag = tagService.GetTag(tagId, false).Result;
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
						var blogposts = blogPostService.GetBlogPosts().Result;
						data["blogposts"] = blogposts.ToArray();
					}
					break;
				case "community-blog-show":
				case "community-blog-edit":
					{
						var blogPostId = Convert.ToInt32(route.Parameters["blogpostid"]);
						var blogPost = blogPostService.GetBlogPost(blogPostId).Result;
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

			return Task.CompletedTask;
		}
	}
}
