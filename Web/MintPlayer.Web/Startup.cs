using System;
using System.Linq;
using System.Threading.Tasks;
using Identity.ExternalProviders.GitHub;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Data.Extensions;
using MintPlayer.Data.Options;
using MintPlayer.Data.Repositories.Interfaces;
using OpenSearch.Extensions;
using SitemapXml;
using Spa.SpaRoutes;
using Spa.SpaRoutes.CurrentSpaRoute;

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
            services.AddMintPlayer(options => {
                options.ConnectionString = @"Server=(localdb)\mssqllocaldb;Database=MintPlayer;Trusted_Connection=True;ConnectRetryCount=0";
                options.JwtIssuerOptions = new Data.Options.JwtIssuerOptions
                {
                    Issuer = Configuration["JwtIssuerOptions:Issuer"],
                    Audience = Configuration["JwtIssuerOptions:Audience"],
                    Subject = Configuration["JwtIssuerOptions:Subject"],
                    ValidFor = Configuration.GetValue<TimeSpan>("JwtIssuerOptions:ValidFor"),
                    Key = Configuration["JwtIssuerOptions:Key"],
                };
                options.FacebookOptions = new FacebookOptions
                {
                    AppId = Configuration["FacebookOptions:AppId"],
                    AppSecret = Configuration["FacebookOptions:AppSecret"]
                };
                options.MicrosoftOptions = new MicrosoftAccountOptions
                {
                    ClientId = Configuration["MicrosoftOptions:AppId"],
                    ClientSecret = Configuration["MicrosoftOptions:AppSecret"]
                };
                options.GoogleOptions = new GoogleOptions
                {
                    ClientId = Configuration["GoogleOptions:AppId"],
                    ClientSecret = Configuration["GoogleOptions:AppSecret"]
                };
                options.TwitterOptions = new TwitterOptions
                {
                    ConsumerKey = Configuration["TwitterOptions:ApiKey"],
                    ConsumerSecret = Configuration["TwitterOptions:ApiSecret"]
                };
                options.GitHubOptions = new GitHubOptions
                {
                    ClientId = Configuration["GithubOptions:ClientId"],
                    ClientSecret = Configuration["GithubOptions:ClientSecret"],
                };
            });

            services.AddElasticSearch(options => {
                options.Url = Configuration["elasticsearch:url"];
                options.DefaultIndex = Configuration["elasticsearch:index"];
            });

            services.AddMvc(options => {
                options.RespectBrowserAcceptHeader = true; // false by default
            })
            .AddXmlSerializerFormatters()
            .AddSitemapXmlFormatters(options => {
                options.Stylesheet = "/assets/sitemap.xsl";
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            // Define the SPA-routes for our helper
            services.AddSpaRoutes(routes => routes
                .Route("", "home")
                .Group("person", "person", person_routes => person_routes
                    .Route("", "list")
                    .Route("create", "create")
                    .Route("{id}", "show")
                    .Route("{id}/edit", "edit")
                )
                .Group("artist", "artist", artist_routes => artist_routes
                    .Route("", "list")
                    .Route("create", "create")
                    .Route("{id}", "show")
                    .Route("{id}/edit", "edit")
                )
                .Group("song", "song", song_routes => song_routes
                    .Route("", "list")
                    .Route("create", "create")
                    .Route("{id}", "show")
                    .Route("{id}/edit", "edit")
                )
            );

            services.AddSitemapXml();

            //services.AddOpenSearch();

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


                }).Configure<RazorViewEngineOptions>(options =>
                {
                    var new_locations = options.ViewLocationFormats.Select(vlf => $"/Server{vlf}").ToList();
                    options.ViewLocationFormats.Clear();
                    foreach (var format in new_locations)
                        options.ViewLocationFormats.Add(format);

                })
                .Configure<JwtIssuerOptions>(options =>
                {
                    options.Issuer = Configuration["JwtIssuerOptions:Issuer"];
                    options.Subject = Configuration["JwtIssuerOptions:Subject"];
                    options.Audience = Configuration["JwtIssuerOptions:Audience"];
                    options.ValidFor = Configuration.GetValue<TimeSpan>("JwtIssuerOptions:ValidFor");
                    options.Key = Configuration["JwtIssuerOptions:Key"];

                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ICurrentSpaRoute currentSpaRoute)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app
                .UseHsts()
                .UseHttpsRedirection()
                .UseForwardedHeaders()
                .UseStaticFiles()
                .UseSpaStaticFiles();

            app.UseOpenSearch(options =>
            {
                options.OsdxEndpoint =  "/opensearch.xml";
                options.SearchUrl = "/api/Subject/opensearch/redirect/{searchTerms}";
                options.SuggestUrl = "/api/Subject/opensearch/suggest/{searchTerms}";
                options.ImageUrl = "/assets/logo/music_note_16.png";
                options.ShortName = "MintPlayer";
                options.Description = "Search music on MintPlayer";
                options.Contact = "pieterjandeclippel@msn.com";
            });

            app.Use((context, next) =>
            {
                if (!context.Request.Headers.ContainsKey("Authorization"))
                {
                    if (context.Request.Cookies.ContainsKey("mintplayer"))
                    {
                        var dataprotectorProvider = context.RequestServices.GetService<IDataProtectionProvider>();
                        var dataprotector = dataprotectorProvider.CreateProtector("Login");
                        var token = dataprotector.Unprotect(context.Request.Cookies["mintplayer"]);
                        context.Request.Headers.Add("Authorization", $"bearer {token}");
                    }
                }
                return next();
            });

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                spa.UseSpaPrerendering(options => {
                    options.BootModulePath = $"{spa.Options.SourcePath}/dist/server/main.js";
                    options.BootModuleBuilder = env.IsDevelopment()
                        ? new AngularCliBuilder(npmScript: "build:ssr")
                        : null;
                    options.ExcludeUrls = new[] { "/sockjs-node" };
                    options.SupplyData = (context, data) =>
                    {
                        var lang = context.Request.Query["lang"];
                        if (lang.Any())
                            data["lang"] = lang.First();
                        else
                            data["lang"] = "en";

                        var route = currentSpaRoute.GetCurrentRoute(context);

                        var personRepository = context.RequestServices.GetRequiredService<IPersonRepository>();
                        var artistRepository = context.RequestServices.GetRequiredService<IArtistRepository>();
                        var songRepository = context.RequestServices.GetRequiredService<ISongRepository>();
                        var mediumTypeRepository = context.RequestServices.GetRequiredService<IMediumTypeRepository>();

                        if (route != null)
                        {
                            switch (route.Name)
                            {
                                case "person-list":
                                    {
                                        var people = personRepository.GetPeople();
                                        data["people"] = people;
                                    }
                                    break;
                                case "person-show":
                                case "person-edit":
                                    {
                                        var id = Convert.ToInt32(route.Parameters["id"]);
                                        var person = personRepository.GetPerson(id, true);
                                        data["person"] = person;
                                    }
                                    break;
                                case "artist-list":
                                    {
                                        var artists = artistRepository.GetArtists();
                                        data["artists"] = artists;
                                    }
                                    break;
                                case "artist-show":
                                case "artist-edit":
                                    {
                                        var id = Convert.ToInt32(route.Parameters["id"]);
                                        var artist = artistRepository.GetArtist(id, true);
                                        data["artist"] = artist;
                                    }
                                    break;
                                case "song-list":
                                    {
                                        var songs = songRepository.GetSongs();
                                        data["songs"] = songs;
                                    }
                                    break;
                                case "song-show":
                                case "song-edit":
                                    {
                                        var id = Convert.ToInt32(route.Parameters["id"]);
                                        var song = songRepository.GetSong(id, true);
                                        data["song"] = song;
                                    }
                                    break;

                                case "mediumtype-list":
                                    {
                                        var mediumTypes = mediumTypeRepository.GetMediumTypes();
                                        data["mediumTypes"] = mediumTypes;
                                    }
                                    break;
                                case "mediumtype-show":
                                case "mediumtype-edit":
                                    {
                                        var id = Convert.ToInt32(route.Parameters["id"]);
                                        var mediumType = mediumTypeRepository.GetMediumType(id, true);
                                        data["mediumType"] = mediumType;
                                    }
                                    break;
                            }
                        }
                    };
                });

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");

                    // Run: 
                    // ng build --prod
                    // http -server -p 9000 -c-1 dist/ClientApp
                    //spa.UseProxyToSpaDevelopmentServer("http://localhost:9000");
                }
            });
        }
    }
}
