#define RebuildSPA

using AspNetCoreOpenSearch.Extensions;
using AspNetCoreOpenSearch.Options;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using MintPlayer.AspNetCore.SitemapXml;
using MintPlayer.AspNetCore.SpaServices.Routing;
using MintPlayer.Data.Extensions;
using MintPlayer.Web.Extensions;
using MintPlayer.Web.Services;
using Spa.SpaRoutes;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebMarkupMin.AspNetCore3;

namespace MintPlayer.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddResponseCompression()
                .AddResponseCaching();
            services
                .AddMintPlayer(options =>
                {
                    options.ConnectionString = Configuration.GetConnectionString("MintPlayer");
                })
                .AddElasticSearch(options =>
                {
                    options.Url = Configuration["ElasticSearch:Url"];
                    options.DefaultIndex = Configuration["ElasticSearch:Index"];
                    options.Active = Configuration.GetValue<bool>("ElasticSearch:Active");
                })
                .AddOpenSearch<OpenSearchService>()
                .AddSitemapXml();

            services
                .AddAuthentication()
                .AddJwtBearer()
                .AddFacebook()
                .AddMicrosoftAccount()
                .AddGoogle()
                .AddTwitter();

            services.AddAuthorization(options =>
            {
            });
            services
                .AddDataProtection()
                .PersistKeysToFileSystem(new System.IO.DirectoryInfo(Configuration["DataProtection:KeysLocation"]))
                .SetDefaultKeyLifetime(TimeSpan.FromDays(Configuration.GetValue<int>("DataProtection:DefaultKeyLifetime")));

            if (Configuration.GetValue<bool>("ElasticSearch:Active"))
            {
                services.AddHostedService<Server.Services.ElasticSearchIndexingService>();
            }

            services.AddHsts(options =>
            {
                options.IncludeSubDomains = true;
                options.Preload = true;
                options.MaxAge = TimeSpan.FromDays(730);
            });

            services.AddHttpContextAccessor();

            services
                .AddControllersWithViews(options =>
                {
                    options.RespectBrowserAcceptHeader = true;
                    options.OutputFormatters.Insert(0, new Microsoft.AspNetCore.Mvc.Formatters.XmlDataContractSerializerOutputFormatter());
                })
                .AddXmlSerializerFormatters()
                .AddSitemapXmlFormatters(options =>
                {
                    options.StylesheetUrl = "/assets/stitemap.xsl";
                })
                .SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddNewtonsoftJson();

            services.AddWebMarkupMin().AddHttpCompression();
            services.AddRouting();

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
            services.AddSpaRoutes(routes => routes
                .Route("", "home")
                .Route("search", "search")
                .Group("account", "account", account_routes => account_routes
                    .Route("login", "login")
                    .Route("register", "register")
                    .Route("profile", "profile")
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
                .Group("playlist", "playlist", playlist_routes => playlist_routes
                    .Route("", "list")
                    .Route("create", "create")
                    .Route("{id}", "show")
                    .Route("{id}/{description}", "show-description")
                    .Route("{id}/edit", "edit")
                    .Route("{id}/{description}/edit", "edit-description")
                )
                .Group("community", "community", community_routes => community_routes
                    .Group("blog", "blog", blog_routes => blog_routes
                        .Route("", "list")
                        .Route("create", "create")
                        .Route("{blogpostid}/{blogpost_title}", "show")
                        .Route("{blogpostid}/{blogpost_title}/edit", "edit")
                    )
                )
            );

            services
                .Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
                })
                .Configure<IdentityOptions>(options =>
                {
                    // Password settings
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = false;
                    options.Password.RequiredUniqueChars = 6;

                    // Lockout settings
                    options.Lockout.DefaultLockoutTimeSpan = System.TimeSpan.FromMinutes(30);
                    options.Lockout.MaxFailedAccessAttempts = 10;
                    options.Lockout.AllowedForNewUsers = true;

                    // User settings
                    options.User.RequireUniqueEmail = true;
                    options.User.AllowedUserNameCharacters = string.Empty;
                })
                //.Configure<MvcOptions>("mvc", options =>
                //{
                //    options.RespectBrowserAcceptHeader = true;
                //    options.OutputFormatters.Insert(0, new Microsoft.AspNetCore.Mvc.Formatters.XmlSerializerOutputFormatter());
                //})
                .Configure<Microsoft.AspNetCore.Routing.RouteOptions>(options =>
                {
                    //options.LowercaseUrls = true;
                })
                .Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Audience = Configuration["JwtIssuerOptions:Audience"];
                    options.ClaimsIssuer = Configuration["JwtIssuerOptions:Issuer"];
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidAudience = Configuration["JwtIssuerOptions:Audience"],
                        ValidateIssuer = true,
                        ValidIssuer = Configuration["JwtIssuerOptions:Issuer"],
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new Func<Microsoft.IdentityModel.Tokens.SymmetricSecurityKey>(() =>
                        {
                            var key = Configuration["JwtIssuerOptions:Key"];
                            var bytes = System.Text.Encoding.UTF8.GetBytes(key);
                            var signing_key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(bytes);
                            return signing_key;
                        }).Invoke(),
                        ValidateLifetime = true
                    };
                    options.SaveToken = false;
                })
                .Configure<Data.Options.JwtIssuerOptions>(options =>
                {
                    options.Audience = Configuration["JwtIssuerOptions:Audience"];
                    options.Issuer = Configuration["JwtIssuerOptions:Issuer"];
                    options.Key = Configuration["JwtIssuerOptions:Key"];
                    options.Subject = Configuration["JwtIssuerOptions:Subject"];
                    options.ValidFor = Configuration.GetValue<TimeSpan>("JwtIssuerOptions:ValidFor");
                })
                .Configure<FacebookOptions>(FacebookDefaults.AuthenticationScheme, options =>
                {
                    options.AppId = Configuration["FacebookOptions:AppId"];
                    options.AppSecret = Configuration["FacebookOptions:AppSecret"];
                })
                .Configure<MicrosoftAccountOptions>(MicrosoftAccountDefaults.AuthenticationScheme, options =>
                {
                    options.ClientId = Configuration["MicrosoftOptions:AppId"];
                    options.ClientSecret = Configuration["MicrosoftOptions:AppSecret"];
                })
                .Configure<GoogleOptions>(GoogleDefaults.AuthenticationScheme, options =>
                {
                    options.ClientId = Configuration["GoogleOptions:AppId"];
                    options.ClientSecret = Configuration["GoogleOptions:AppSecret"];
                })
                .Configure<TwitterOptions>(TwitterDefaults.AuthenticationScheme, options =>
                {
                    options.ConsumerKey = Configuration["TwitterOptions:ApiKey"];
                    options.ConsumerSecret = Configuration["TwitterOptions:ApiSecret"];
                    options.RetrieveUserDetails = true;
                })
                .Configure<RazorViewEngineOptions>(options =>
                {
                    var new_locations = options.ViewLocationFormats.Select(vlf => $"/Server{vlf}").ToList();
                    options.ViewLocationFormats.Clear();
                    foreach (var format in new_locations)
                        options.ViewLocationFormats.Add(format);
                })
                .Configure<StaticFileOptions>(options =>
                {
                    options.OnPrepareResponse = (context) =>
                    {
                        if (context.File.Name == "index.html")
                        {
                            context.Context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
                            {
                                // Cache-Control:private,no-store,max-age=0,no-cache,must-revalidate
                                Private = true,
                                NoStore = true,
                                MaxAge = TimeSpan.Zero,
                                NoCache = true,
                                MustRevalidate = true
                            };
                        }
                    };
                })
                .Configure<OpenSearchOptions>(options =>
                {
                    options.OsdxEndpoint = "/opensearch.xml";
                    options.SearchUrl = "/web/OpenSearch/redirect/{searchTerms}";
                    options.SuggestUrl = "/web/OpenSearch/suggest/{searchTerms}";
                    options.ImageUrl = "/assets/logo/music_note_16.png";
                    options.ShortName = "MintPlayer";
                    options.Description = "Search music on MintPlayer";
                    options.Contact = "info@mintplayer.com";
                })
                //.Configure<HstsOptions>(options =>
                //{
                //    options.IncludeSubDomains = true;
                //    options.Preload = true;
                //    options.MaxAge = TimeSpan.FromDays(730);
                //})
                .Configure<WebMarkupMinOptions>(options =>
                {
                    options.DisablePoweredByHttpHeaders = true;
                    options.AllowMinificationInDevelopmentEnvironment = true;
                    options.AllowCompressionInDevelopmentEnvironment = true;
                    options.DisablePoweredByHttpHeaders = false;
                })
                .Configure<HtmlMinificationOptions>(options =>
                {
                    options.MinificationSettings.RemoveEmptyAttributes = true;
                    options.MinificationSettings.RemoveRedundantAttributes = true;
                    options.MinificationSettings.RemoveHttpProtocolFromAttributes = true;
                    options.MinificationSettings.RemoveHttpsProtocolFromAttributes = false;
                    options.MinificationSettings.MinifyInlineJsCode = true;
                    options.MinificationSettings.MinifyEmbeddedJsCode = true;
                    options.MinificationSettings.MinifyEmbeddedJsonData = true;
                    options.MinificationSettings.WhitespaceMinificationMode = WebMarkupMin.Core.WhitespaceMinificationMode.Aggressive;
                })
                .ConfigureApplicationCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromDays(3);
#if RELEASE
                    options.Cookie.Domain = ".mintplayer.com";
#endif
                    options.Events.OnRedirectToLogin = (context) =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISpaRouteService spaRouteService)
        {
            app.Use(async (context, next) =>
            {
                await next();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseOpenSearch();
            app.UseDefaultSitemapXmlStylesheet(options =>
            {
                options.StylesheetUrl = "/assets/stitemap.xsl";
            });

            app.UseAuthentication();

            app.UseWhen(
                context => context.Request.Path.StartsWithSegments("/amp"),
                appBuilder =>
                {
                    appBuilder.UseWebMarkupMin();
                }
            );
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseResponseCaching();
            app.Use(async (context, next) =>
            {
                context.Response.OnStarting(() =>
                {
                    if (!context.Response.Headers.ContainsKey("cache-control"))
                        context.Response.Headers.Add("cache-control", "no-cache");
                    return Task.CompletedTask;
                });
                //if (context.Request.Path == "/index.html")
                //{
                //    context.Response.Headers.Add("Cache-control", "no-cache, no-store, must-revalidate");
                //    context.Response.Headers.Add("Pragma", "no-cache");
                //}
                await next();
            });

            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles(new StaticFileOptions
                {
                    //OnPrepareResponse = (context) =>
                    //{
                    //    const int duration = 7 * 60 * 60 * 24;
                    //    var expires = DateTime.UtcNow.AddSeconds(duration).ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                    //    context.Context.Response.Headers[HeaderNames.CacheControl] = $"public,max-age={duration}";
                    //    context.Context.Response.Headers[HeaderNames.Expires] = expires + " GMT";
                    //}
                });
            }

            // Redirect subject urls
            app.Use(async (context, next) =>
            {
                var route = spaRouteService.GetCurrentRoute(context);
                switch (route?.Name)
                {
                    case "person-show":
                        {
                            var id = Convert.ToInt32(route.Parameters["id"]);
                            var personService = context.RequestServices.GetRequiredService<Data.Services.IPersonService>();
                            var person = await personService.GetPerson(id, false, false);
                            if (person == null)
                            {
                                context.Response.OnStarting(() =>
                                {
                                    context.Response.StatusCode = 404;
                                    return Task.CompletedTask;
                                });
                                await next();
                            }
                            else
                            {
                                context.Response.Redirect(spaRouteService.GenerateUrl("person-show-name", new { id = id, name = person.Text.Slugify() }));
                            }
                        }
                        break;
                    case "artist-show":
                        {
                            var id = Convert.ToInt32(route.Parameters["id"]);
                            var artistService = context.RequestServices.GetRequiredService<Data.Services.IArtistService>();
                            var artist = await artistService.GetArtist(id, false, false);
                            if (artist == null)
                            {
                                context.Response.OnStarting(() =>
                                {
                                    context.Response.StatusCode = 404;
                                    return Task.CompletedTask;
                                });
                                await next();
                            }
                            else
                            {
                                context.Response.Redirect(spaRouteService.GenerateUrl("artist-show-name", new { id = id, name = artist.Name.Slugify() }));
                            }
                        }
                        break;
                    case "song-show":
                        {
                            var id = Convert.ToInt32(route.Parameters["id"]);
                            var songService = context.RequestServices.GetRequiredService<Data.Services.ISongService>();
                            var song = await songService.GetSong(id, false, false);
                            if (song == null)
                            {
                                context.Response.OnStarting(() =>
                                {
                                    context.Response.StatusCode = 404;
                                    return Task.CompletedTask;
                                });
                                await next();
                            }
                            else
                            {
                                context.Response.Redirect(spaRouteService.GenerateUrl("song-show-title", new { id = id, title = song.Title.Slugify() }));
                            }
                        }
                        break;
                    case "playlist-show":
                        {
                            var id = Convert.ToInt32(route.Parameters["id"]);
                            var playlistService = context.RequestServices.GetRequiredService<Data.Services.IPlaylistService>();
                            var playlist = await playlistService.GetPlaylist(id);
                            if (playlist == null)
                            {
                                context.Response.OnStarting(() =>
                                {
                                    context.Response.StatusCode = 404;
                                    return Task.CompletedTask;
                                });
                                await next();
                            }
                            else
                            {
                                context.Response.Redirect(spaRouteService.GenerateUrl("playlist-show-description", new { id = id, description = playlist.Description.Slugify() }));
                            }
                        }
                        break;
                    case "person-show-name":
                    case "person-edit-name":
                        {
                            var id = Convert.ToInt32(route.Parameters["id"]);
                            var personService = context.RequestServices.GetRequiredService<Data.Services.IPersonService>();
                            var person = await personService.GetPerson(id, false, false);
                            if (person == null)
                            {
                                context.Response.OnStarting(() =>
                                {
                                    context.Response.StatusCode = 404;
                                    return Task.CompletedTask;
                                });
                                await next();
                            }
                            else if (route.Parameters["name"] == person.Text.Slugify())
                            {
                                await next();
                            }
                            else
                            {
                                context.Response.Redirect(spaRouteService.GenerateUrl(route.Name, new { id = id, name = person.Text.Slugify() }));
                            }
                        }
                        break;
                    case "artist-show-name":
                    case "artist-edit-name":
                        {
                            var id = Convert.ToInt32(route.Parameters["id"]);
                            var artistService = context.RequestServices.GetRequiredService<Data.Services.IArtistService>();
                            var artist = await artistService.GetArtist(id, false, false);
                            if (artist == null)
                            {
                                context.Response.OnStarting(() =>
                                {
                                    context.Response.StatusCode = 404;
                                    return Task.CompletedTask;
                                });
                                await next();
                            }
                            else if (route.Parameters["name"] == artist.Text.Slugify())
                            {
                                await next();
                            }
                            else
                            {
                                context.Response.Redirect(spaRouteService.GenerateUrl(route.Name, new { id = id, name = artist.Text.Slugify() }));
                            }
                        }
                        break;
                    case "song-show-title":
                    case "song-edit-title":
                        {
                            var id = Convert.ToInt32(route.Parameters["id"]);
                            var songService = context.RequestServices.GetRequiredService<Data.Services.ISongService>();
                            var song = await songService.GetSong(id, false, false);
                            if (song == null)
                            {
                                context.Response.OnStarting(() =>
                                {
                                    context.Response.StatusCode = 404;
                                    return Task.CompletedTask;
                                });
                                await next();
                            }
                            else if (route.Parameters["title"] == song.Text.Slugify())
                            {
                                await next();
                            }
                            else
                            {
                                context.Response.Redirect(spaRouteService.GenerateUrl(route.Name, new { id = id, title = song.Text.Slugify() }));
                            }
                        }
                        break;
                    case "playlist-show-description":
                    case "playlist-edit-description":
                        {
                            var id = Convert.ToInt32(route.Parameters["id"]);
                            var playlistService = context.RequestServices.GetRequiredService<Data.Services.IPlaylistService>();
                            var playlist = await playlistService.GetPlaylist(id);
                            if (playlist == null)
                            {
                                context.Response.OnStarting(() =>
                                {
                                    context.Response.StatusCode = 404;
                                    return Task.CompletedTask;
                                });
                                await next();
                            }
                            else if (route.Parameters["description"] == playlist.Description.Slugify())
                            {
                                await next();
                            }
                            else
                            {
                                context.Response.Redirect(spaRouteService.GenerateUrl(route.Name, new { id = id, description = playlist.Description.Slugify() }));
                            }
                        }
                        break;
                    default:
                        await next();
                        break;
                }
            });

            app.UseWhen(
                context => ShouldUseSpa(context),
                app2 => app2.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "ClientApp";

                    //spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions
                    //{
                    //    OnPrepareResponse = (context) =>
                    //    {
                    //        const int duration = 60 * 60 * 24;
                    //        var expires = DateTime.UtcNow.AddSeconds(duration).ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                    //        context.Context.Response.Headers[HeaderNames.CacheControl] = $"public,max-age={duration}";
                    //        context.Context.Response.Headers[HeaderNames.Expires] = expires + " GMT";
                    //    }
                    //};

#pragma warning disable CS0618 // Type or member is obsolete
                    spa.UseSpaPrerendering(options =>
                    {
                        options.BootModulePath = $"{spa.Options.SourcePath}/dist/server/main.js";
#if RebuildSPA
                        options.BootModuleBuilder = env.IsDevelopment()
                            ? new AngularCliBuilder(npmScript: "build:ssr")
                            : null;
#else
                        options.BootModuleBuilder = null;
#endif
                        options.ExcludeUrls = new[] { "/sockjs-node" };
                        options.SupplyData = async (context, data) =>
                        {
                            var route = spaRouteService.GetCurrentRoute(context);
                            var personService = context.RequestServices.GetRequiredService<Data.Services.IPersonService>();
                            var artistService = context.RequestServices.GetRequiredService<Data.Services.IArtistService>();
                            var songService = context.RequestServices.GetRequiredService<Data.Services.ISongService>();
                            var mediumTypeService = context.RequestServices.GetRequiredService<Data.Services.IMediumTypeService>();
                            var accountService = context.RequestServices.GetRequiredService<Data.Services.IAccountService>();
                            var tagCategoryService = context.RequestServices.GetRequiredService<Data.Services.ITagCategoryService>();
                            var tagService = context.RequestServices.GetRequiredService<Data.Services.ITagService>();
                            var playlistService = context.RequestServices.GetRequiredService<Data.Services.IPlaylistService>();
                            var blogPostService = context.RequestServices.GetRequiredService<Data.Services.IBlogPostService>();

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
                                        var req = new Pagination.PaginationRequest<Dtos.Dtos.Person> { PerPage = 20, Page = 1, SortProperty = "FirstName", SortDirection = System.ComponentModel.ListSortDirection.Ascending };
                                        var people = await personService.PagePeople(req);
                                        data["people"] = people;
                                    }
                                    break;
                                case "person-create":
                                    {
                                        var mediumtypes = await mediumTypeService.GetMediumTypes(false, false);
                                        data["mediumtypes"] = mediumtypes.ToArray();
                                    }
                                    break;
                                case "person-show":
                                case "person-edit":
                                    {
                                        var id = System.Convert.ToInt32(route.Parameters["id"]);
                                        var person = await personService.GetPerson(id, true, false);
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
                                        var mediumtypes = await mediumTypeService.GetMediumTypes(false, false);
                                        data["mediumtypes"] = mediumtypes.ToArray();
                                    }
                                    break;
                                case "artist-list":
                                    {
                                        var req = new Pagination.PaginationRequest<Dtos.Dtos.Artist> { PerPage = 20, Page = 1, SortProperty = "Name", SortDirection = System.ComponentModel.ListSortDirection.Ascending };
                                        var artists = await artistService.PageArtists(req);
                                        data["artists"] = artists;
                                    }
                                    break;
                                case "artist-create":
                                    {
                                        var mediumtypes = await mediumTypeService.GetMediumTypes(false, false);
                                        data["mediumtypes"] = mediumtypes.ToArray();
                                    }
                                    break;
                                case "artist-show":
                                case "artist-edit":
                                    {
                                        var id = System.Convert.ToInt32(route.Parameters["id"]);
                                        var artist = await artistService.GetArtist(id, true, false);
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
                                        var mediumtypes = await mediumTypeService.GetMediumTypes(false, false);
                                        data["mediumtypes"] = mediumtypes.ToArray();
                                    }
                                    break;
                                case "song-list":
                                    {
                                        var req = new Pagination.PaginationRequest<Dtos.Dtos.Song> { PerPage = 20, Page = 1, SortProperty = "Title", SortDirection = System.ComponentModel.ListSortDirection.Ascending };
                                        var songs = await songService.PageSongs(req);
                                        data["songs"] = songs;
                                    }
                                    break;
                                case "song-create":
                                    {
                                        var mediumtypes = await mediumTypeService.GetMediumTypes(false, false);
                                        data["mediumtypes"] = mediumtypes.ToArray();
                                    }
                                    break;
                                case "song-show":
                                case "song-edit":
                                    {
                                        var id = System.Convert.ToInt32(route.Parameters["id"]);
                                        var song = await songService.GetSong(id, true, false);
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
                                        var mediumtypes = await mediumTypeService.GetMediumTypes(false, false);
                                        data["mediumtypes"] = mediumtypes.ToArray();
                                    }
                                    break;
                                case "mediumtype-list":
                                    {
                                        var mediumtypes = await mediumTypeService.GetMediumTypes(false, false);
                                        data["mediumtypes"] = mediumtypes.ToArray();
                                    }
                                    break;
                                case "mediumtype-create":
                                    break;
                                case "mediumtype-show":
                                case "mediumtype-edit":
                                    {
                                        var id = System.Convert.ToInt32(route.Parameters["id"]);
                                        var mediumtype = await mediumTypeService.GetMediumType(id, false, false);
                                        if (mediumtype == null)
                                            context.Response.OnStarting(() =>
                                            {
                                                context.Response.StatusCode = 404;
                                                return Task.CompletedTask;
                                            });
                                        else
                                            data["mediumtype"] = mediumtype;
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
                                //        var playlists = await playlistService.PagePlaylists(req);
                                //        data["playlists"] = playlists;
                                //    }
                                //    break;
                                //case "playlist-show":
                                //case "playlist-edit":
                                //    {
                                //        var id = System.Convert.ToInt32(route.Parameters["id"]);
                                //        var playlist = await playlistService.GetPlaylist(id, true);
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
                        };
                    });
#pragma warning restore CS0618 // Type or member is obsolete

                    app2.UseWebMarkupMin();

                    if (env.IsDevelopment())
                    {
                        spa.UseAngularCliServer(npmScript: "start");
                    }
                })
            );
        }

        /// <summary>Determines whether the request belongs to the root domain, and not to a subdomain.</summary>
        /// <param name="context">HTTP context</param>
        private bool ShouldUseSpa(HttpContext context)
        {
#if RELEASE
            return context.Request.Host.Host == Configuration["Website:Host"];
#else
            return true;
#endif
        }
    }
}