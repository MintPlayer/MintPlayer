//#define RebuildSPA

using AspNetCoreOpenSearch.Extensions;
using AspNetCoreOpenSearch.Options;
using Microsoft.AspNetCore.Antiforgery;
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
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using MintPlayer.AspNetCore.SitemapXml;
using MintPlayer.AspNetCore.SpaServices.Routing;
using MintPlayer.AspNetCore.XsrfForSpas;
using MintPlayer.Data.Extensions;
using MintPlayer.Fetcher.Integration.Extensions;
using MintPlayer.Pagination;
using MintPlayer.Web.Extensions;
using MintPlayer.Web.Services;
using System;
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
				.AddFetcherIntegration()
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
                .AddTwitter()
				.AddLinkedIn();

            services.AddAuthorization(options =>
            {
            });

            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

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

            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicies.AllowDatatables, policy =>
                {
                    policy
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithMethods(HttpMethods.Post)
                        .SetIsOriginAllowed((origin) => Regex.IsMatch(origin, @"http[s]{0,1}\:\/\/localhost\:[0-9]+"));
                });
            });

            services
                .AddControllersWithViews(options =>
                {
                    options.RespectBrowserAcceptHeader = true;
                })
                .AddXmlDataContractSerializerFormatters()
                .AddSitemapXmlFormatters(options =>
                {
                    options.StylesheetUrl = "/assets/stitemap.xsl";
                })
                .AddMvcOptions(options =>
                {
                    options.OutputFormatters.OfType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonOutputFormatter>().FirstOrDefault().SupportedMediaTypes.Add("text/plain");
                    options.OutputFormatters.Remove(options.OutputFormatters.OfType<Microsoft.AspNetCore.Mvc.Formatters.StringOutputFormatter>().FirstOrDefault());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddNewtonsoftJson();

            services.AddWebMarkupMin().AddHttpCompression();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "MintPlayer API",
                    Version = "v1",
                    Description = "API that allows access to the data of MintPlayer. "
                });

                // Set the comments path for the Swagger JSON and UI.**
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });
            services.AddRouting();

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
            services.AddSpaPrerenderingService<Services.SpaRouteService>(routes => routes
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
                )
            );

			services
				.AddFetcherContainer()
				.AddGeniusFetcher();

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
				.Configure<AspNet.Security.OAuth.LinkedIn.LinkedInAuthenticationOptions>(AspNet.Security.OAuth.LinkedIn.LinkedInAuthenticationDefaults.AuthenticationScheme, options =>
				{
					options.ClientId = Configuration["LinkedInOptions:AppId"];
					options.ClientSecret = Configuration["LinkedInOptions:AppSecret"];
					options.ReturnUrlParameter = "/signin-linkedin";
				})
                .Configure<Data.Options.SmtpOptions>(options =>
                {
                    options.Host = Configuration["Mail:Host"];
                    options.Port = Configuration.GetValue<int>("Mail:Port");
                    options.UseTLS = Configuration.GetValue<bool>("Mail:Tls");
                    options.User = Configuration["Mail:User"];
                    options.Password = Configuration["Mail:Password"];
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

							//const int duration = 60 * 60 * 24;
							//var expires = DateTime.UtcNow.AddSeconds(duration).ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

							////context.Context.Response.Headers[HeaderNames.CacheControl] = $"public,max-age={duration}";
							//context.Context.Response.Headers[HeaderNames.Expires] = expires + " GMT";

							//context.Context.Response.Headers.Add("Strict-Transport-Security", $"max-age={TimeSpan.FromDays(730).TotalMilliseconds}; preload; includeSubDomains");

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
                    //options.Cookie.Domain = ".mintplayer.com";
                    options.Cookie.Domain = "mintplayer.com";
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
            //app.Use(async (context, next) =>
            //{
            //    var logEntryService = context.RequestServices.GetRequiredService<Data.Services.ILogEntryService>();
            //    await logEntryService.Log($"Processing request. BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");
            //    await next();
            //});

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
            app.UseSwagger(options =>
            {
            });
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "MintPlayer API V1");
            });
            app.UseAntiforgery();
            app.UseOpenSearch();

            app.UseAuthentication();

            app.UseWhen(
                context => context.Request.Path.StartsWithSegments("/amp"),
                appBuilder =>
                {
                    appBuilder.UseWebMarkupMin();
                }
            );
            app.UseRouting();
            app.UseCors();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}")
                .RequireCors("AllowPage");

                endpoints.MapDefaultSitemapXmlStylesheet(options =>
                {
                    options.StylesheetUrl = "/assets/stitemap.xsl";
                });
            });

            //app.UseResponseCaching();
            //app.Use(async (context, next) =>
            //{
            //    context.Response.OnStarting(() =>
            //    {
            //        if (!context.Response.Headers.ContainsKey("cache-control"))
            //            context.Response.Headers.Add("cache-control", "no-cache, no-store, must-revalidate");

            //        return Task.CompletedTask;
            //    });
            //    await next();
            //});

            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles(new StaticFileOptions
				{
					//OnPrepareResponse = (context) =>
					//{
					//	const int duration = 7 * 60 * 60 * 24;
					//	var expires = DateTime.UtcNow.AddSeconds(duration).ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

					//	context.Context.Response.Headers[HeaderNames.CacheControl] = $"public,max-age={duration}";
					//	context.Context.Response.Headers[HeaderNames.Expires] = expires + " GMT";

					//	context.Context.Response.Headers.Add("Strict-Transport-Security", $"max-age={hstsOptions.Value.MaxAge.TotalMilliseconds}; preload; includeSubDomains");
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
                            var personService = context.RequestServices.GetRequiredService<Data.Abstractions.Services.IPersonService>();
                            var person = await personService.GetPerson(id, false);
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
                            var artistService = context.RequestServices.GetRequiredService<Data.Abstractions.Services.IArtistService>();
                            var artist = await artistService.GetArtist(id, false);
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
                            var songService = context.RequestServices.GetRequiredService<Data.Abstractions.Services.ISongService>();
                            var song = await songService.GetSong(id, false);
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
                            var playlistService = context.RequestServices.GetRequiredService<Data.Abstractions.Services.IPlaylistService>();
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
                            var personService = context.RequestServices.GetRequiredService<Data.Abstractions.Services.IPersonService>();
                            var person = await personService.GetPerson(id, false);
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
                            var artistService = context.RequestServices.GetRequiredService<Data.Abstractions.Services.IArtistService>();
                            var artist = await artistService.GetArtist(id, false);
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
                            var songService = context.RequestServices.GetRequiredService<Data.Abstractions.Services.ISongService>();
                            var song = await songService.GetSong(id, false);
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
                            var playlistService = context.RequestServices.GetRequiredService<Data.Abstractions.Services.IPlaylistService>();
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
					//	OnPrepareResponse = (context) =>
					//	{
					//		const int duration = 60 * 60 * 24;
					//		var expires = DateTime.UtcNow.AddSeconds(duration).ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

					//		context.Context.Response.Headers[HeaderNames.CacheControl] = $"public,max-age={duration}";
					//		context.Context.Response.Headers[HeaderNames.Expires] = expires + " GMT";

					//		context.Context.Response.Headers.Add("Strict-Transport-Security", $"max-age=63072000; preload; includeSubDomains");
					//	}
					//};

#pragma warning disable CS0618 // Type or member is obsolete
					spa.UseSpaPrerenderingService(options =>
                    {
#if RebuildSPA
                        options.BootModuleBuilder = env.IsDevelopment() ? new AngularCliBuilder(npmScript: "build:ssr") : null;
#else
                        options.BootModuleBuilder = null;
#endif
                        options.BootModulePath = $"{spa.Options.SourcePath}/dist/server/main.js";
                        options.ExcludeUrls = new[] { "/sockjs-node" };
                    });
#pragma warning restore CS0618 // Type or member is obsolete

                    //app2.UseWebMarkupMin();

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
