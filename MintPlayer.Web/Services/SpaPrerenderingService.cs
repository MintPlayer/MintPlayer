using MintPlayer.AspNetCore.SpaServices.Prerendering.Services;
using MintPlayer.AspNetCore.SpaServices.Routing;
using MintPlayer.Data.Abstractions.Services;
using MintPlayer.Pagination;
using MintPlayer.Web.Extensions;

namespace MintPlayer.Web.Services;

public class SpaRouteService : ISpaPrerenderingService
{
	#region Constructor
	private readonly ISpaRouteService spaRouteService;
	private readonly IPersonService personService;
	private readonly IArtistService artistService;
	private readonly ISongService songService;
	private readonly IMediumTypeService mediumTypeService;
	private readonly IAccountService accountService;
	private readonly ITagCategoryService tagCategoryService;
	private readonly ITagService tagService;
	private readonly IPlaylistService playlistService;
	private readonly IBlogPostService blogPostService;
	public SpaRouteService(
		ISpaRouteService spaRouteService,
		IPersonService personService,
		IArtistService artistService,
		ISongService songService,
		IMediumTypeService mediumTypeService,
		IAccountService accountService,
		ITagCategoryService tagCategoryService,
		ITagService tagService,
		IPlaylistService playlistService,
		IBlogPostService blogPostService)
	{
		this.spaRouteService = spaRouteService;
		this.personService = personService;
		this.artistService = artistService;
		this.songService = songService;
		this.mediumTypeService = mediumTypeService;
		this.accountService = accountService;
		this.tagCategoryService = tagCategoryService;
		this.tagService = tagService;
		this.playlistService = playlistService;
		this.blogPostService = blogPostService;
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
		var user = await accountService.GetCurrentUser(context.User);
		data["user"] = user;

		switch (route?.Name)
		{
			case null:
				{
					context.Response.OnStarting(() =>
					{
						context.Response.StatusCode = StatusCodes.Status404NotFound;
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
			case "person-show":
			case "person-edit":
				{
					var id = Convert.ToInt32(route.Parameters["id"]);
					var person = await personService.GetPerson(id, false);
					if (person == null)
					{
						context.Response.OnStarting(() =>
						{
							context.Response.StatusCode = StatusCodes.Status404NotFound;
							return Task.CompletedTask;
						});
					}
					else
					{
						var url = await spaRouteService.GenerateUrl($"{route.Name}-name", new { id = id, name = person.Text.Slugify() });
						context.Response.Redirect(url);
					}
				}
				break;
			case "person-show-name":
			case "person-edit-name":
				{
					var id = Convert.ToInt32(route.Parameters["id"]);
					var person = await personService.GetPerson(id, true);
					if (person == null)
					{
						context.Response.OnStarting(() =>
						{
							context.Response.StatusCode = StatusCodes.Status404NotFound;
							return Task.CompletedTask;
						});
					}
					else if (route.Parameters["name"] == person.Text.Slugify())
					{
						data["person"] = person;
						context.Response.OnStarting(() =>
						{
							context.Response.Headers["last-modified"] = person.DateUpdate.ToISOString();
							return Task.CompletedTask;
						});
					}
					else
					{
						var url = await spaRouteService.GenerateUrl(route.Name, new { id = id, name = person.Text.Slugify() });
						context.Response.Redirect(url);
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
			case "artist-show":
			case "artist-edit":
				{
					var id = Convert.ToInt32(route.Parameters["id"]);
					var artist = await artistService.GetArtist(id, false);
					if (artist == null)
					{
						context.Response.OnStarting(() =>
						{
							context.Response.StatusCode = StatusCodes.Status404NotFound;
							return Task.CompletedTask;
						});
					}
					else
					{
						var url = await spaRouteService.GenerateUrl($"{route.Name}-name", new { id = id, name = artist.Name.Slugify() });
						context.Response.Redirect(url);
					}
				}
				break;
			case "artist-show-name":
			case "artist-edit-name":
				{
					var id = Convert.ToInt32(route.Parameters["id"]);
					var artist = await artistService.GetArtist(id, true);
					if (artist == null)
					{
						context.Response.OnStarting(() =>
						{
							context.Response.StatusCode = StatusCodes.Status404NotFound;
							return Task.CompletedTask;
						});
					}
					else if (route.Parameters["name"] == artist.Text.Slugify())
					{
						data["artist"] = artist;
						context.Response.OnStarting(() =>
						{
							context.Response.Headers["last-modified"] = artist.DateUpdate.ToISOString();
							return Task.CompletedTask;
						});
					}
					else
					{
						var url = await spaRouteService.GenerateUrl(route.Name, new { id = id, name = artist.Text.Slugify() });
						context.Response.Redirect(url);
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
			case "song-show":
			case "song-edit":
				{
					var id = Convert.ToInt32(route.Parameters["id"]);
					var song = await songService.GetSong(id, false);
					if (song == null)
					{
						context.Response.OnStarting(() =>
						{
							context.Response.StatusCode = StatusCodes.Status404NotFound;
							return Task.CompletedTask;
						});
					}
					else
					{
						context.Response.OnStarting(async () =>
						{
							var url = await spaRouteService.GenerateUrl($"{route.Name}-title", new { id = id, title = song.Title.Slugify() });
							context.Response.Redirect(url);
						});
					}
				}
				break;
			case "song-show-title":
			case "song-edit-title":
				{
					var id = Convert.ToInt32(route.Parameters["id"]);
					var song = await songService.GetSong(id, true);
					if (song == null)
					{
						context.Response.OnStarting(() =>
						{
							context.Response.StatusCode = StatusCodes.Status404NotFound;
							return Task.CompletedTask;
						});
					}
					else if (route.Parameters["title"] == song.Text.Slugify())
					{
						data["song"] = song;
						context.Response.OnStarting(() =>
						{
							context.Response.Headers["last-modified"] = song.DateUpdate.ToISOString();
							return Task.CompletedTask;
						});
					}
					else
					{
						var url = await spaRouteService.GenerateUrl(route.Name, new { id = id, title = song.Text.Slugify() });
						context.Response.Redirect(url);
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
					var mediumType = await mediumTypeService.GetMediumType(id, false);
					if (mediumType == null)
					{
						context.Response.OnStarting(() =>
						{
							context.Response.StatusCode = StatusCodes.Status404NotFound;
							return Task.CompletedTask;
						});
					}
					else
					{
						var url = await spaRouteService.GenerateUrl($"{route.Name}-name", new { id = id, name = mediumType.Description.Slugify() });
						context.Response.Redirect(url);
					}
				}
				break;
			case "mediumtype-show-name":
			case "mediumtype-edit-name":
				{
					var id = Convert.ToInt32(route.Parameters["id"]);
					var mediumtype = await mediumTypeService.GetMediumType(id, false);
					if (mediumtype == null)
					{
						context.Response.OnStarting(() =>
						{
							context.Response.StatusCode = StatusCodes.Status404NotFound;
							return Task.CompletedTask;
						});
					}
					else if (route.Parameters["name"] == mediumtype.Description.Slugify())
					{
						data["mediumtype"] = mediumtype;
					}
					else
					{
						var url = await spaRouteService.GenerateUrl(route.Name, new { id = id, name = mediumtype.Description.Slugify() });
						context.Response.Redirect(url);
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
							context.Response.StatusCode = StatusCodes.Status404NotFound;
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
							context.Response.StatusCode = StatusCodes.Status404NotFound;
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
							context.Response.StatusCode = StatusCodes.Status404NotFound;
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
							context.Response.StatusCode = StatusCodes.Status404NotFound;
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
							context.Response.StatusCode = StatusCodes.Status404NotFound;
							return Task.CompletedTask;
						});
					else
						data["blogpost"] = blogPost;
				}
				break;
			//case "playlist-list":
			//    {
			//        var req = new Pagination.PaginationRequest<Data.Dtos.Playlist> { PerPage = 20, Page = 1, SortProperty = "Description", SortDirection = System.ComponentModel.ListSortDirection.Ascending };
			//        var playlists = playlistService.PagePlaylists(req);
			//        data["playlists"] = playlists;
			//    }
			//    break;
			case "playlist-show":
			case "playlist-edit":
				{
					var id = Convert.ToInt32(route.Parameters["id"]);
					var playlist = await playlistService.GetPlaylist(id, true);
					if (playlist == null)
					{
						context.Response.OnStarting(() =>
						{
							context.Response.StatusCode = StatusCodes.Status404NotFound;
							return Task.CompletedTask;
						});
					}
					else if ((playlist.User.Id != user?.Id) && (playlist.Accessibility == Dtos.Enums.EPlaylistAccessibility.Private))
					{
						context.Response.OnStarting(() =>
						{
							context.Response.StatusCode = StatusCodes.Status403Forbidden;
							return Task.CompletedTask;
						});
					}
					else
					{
						var url = await spaRouteService.GenerateUrl($"{route.Name}-description", new { id = id, description = playlist.Description.Slugify() });
						context.Response.Redirect(url);
					}
				}
				break;
			case "playlist-show-description":
			case "playlist-edit-description":
				{
					var id = Convert.ToInt32(route.Parameters["id"]);
					var playlist = await playlistService.GetPlaylist(id);
					if (playlist == null)
					{
						context.Response.OnStarting(() =>
						{
							context.Response.StatusCode = StatusCodes.Status404NotFound;
							return Task.CompletedTask;
						});
					}
					else if ((playlist.User.Id != user?.Id) && (playlist.Accessibility == Dtos.Enums.EPlaylistAccessibility.Private))
					{
						context.Response.OnStarting(() =>
						{
							context.Response.StatusCode = StatusCodes.Status403Forbidden;
							return Task.CompletedTask;
						});
					}
					else if (route.Parameters["description"] == playlist.Description.Slugify())
					{
						data["playlist"] = playlist;
					}
					else
					{
						var url = await spaRouteService.GenerateUrl(route.Name, new { id = id, description = playlist.Description.Slugify() });
						context.Response.Redirect(url);
					}
				}
				break;
			default:
				break;
		}
	}
}
