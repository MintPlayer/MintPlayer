﻿//#define RebuildSPA

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using MintPlayer.AspNetCore.ChangePassword;
using MintPlayer.AspNetCore.NoSniff;
using MintPlayer.AspNetCore.OpenSearch;
using MintPlayer.AspNetCore.SitemapXml;
using MintPlayer.AspNetCore.SpaServices.Extensions;
using MintPlayer.AspNetCore.SpaServices.Prerendering;
using MintPlayer.AspNetCore.SpaServices.Routing;
using MintPlayer.Data.Extensions;
using MintPlayer.Fetcher.Integration.Extensions;
using MintPlayer.Web.Server.Middleware;
using MintPlayer.Web.Services;
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
				.AddSitemapXml(options => options.StylesheetUrl = "/assets/stitemap.xsl");

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
#if RELEASE
				//options.ExcludedHosts.Remove("127.0.0.1");
				options.ExcludedHosts.Clear();
#endif
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
						.SetIsOriginAllowed((origin) => Regex.IsMatch(origin, @"(http[s]{0,1}\:\/\/localhost\:[0-9]+)|(https\:\/\/[a-z]+\.mintplayer\.com)"));
				});
			});

			services
				.AddControllersWithViews()
				.AddXmlDataContractSerializerFormatters()
				.AddMvcOptions(options =>
				{
					options.RespectBrowserAcceptHeader = true;
					options.OutputFormatters.OfType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonOutputFormatter>().FirstOrDefault().SupportedMediaTypes.Add("text/plain");
					options.OutputFormatters.Remove(options.OutputFormatters.OfType<Microsoft.AspNetCore.Mvc.Formatters.StringOutputFormatter>().FirstOrDefault());
				})
				.AddNewtonsoftJson();

			services.AddWebMarkupMin().AddHttpCompression().AddHtmlMinification();
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
			services.AddSpaStaticFilesImproved(configuration =>
			{
				configuration.RootPath = "ClientApp/dist";
			});

			services.AddSpaPrerenderingService<SpaRouteService>();

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

							const int duration = 60 * 60 * 24;
							var expires = DateTime.UtcNow.AddSeconds(duration).ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

							context.Context.Response.Headers[HeaderNames.CacheControl] = $"public,max-age={duration}";
							context.Context.Response.Headers[HeaderNames.Expires] = expires + " GMT";

							context.Context.Response.Headers.Add("Strict-Transport-Security", $"max-age={TimeSpan.FromDays(730).TotalMilliseconds}; preload; includeSubDomains");
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
			app.UseImprovedHsts();
			app.UseHttpsRedirection();
			app.UseNoSniff();
			app.UseSwagger();
			app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "MintPlayer API V1"));
			MintPlayer.AspNetCore.SpaServices.Xsrf.AntiforgeryExtensions.UseAntiforgery(app);
			//app.UseAntiforgery();

			app.UseAuthentication();

			app.UseWhen(
				context => context.Request.Path.StartsWithSegments("/amp"),
				appBuilder =>
				{
					appBuilder.UseWebMarkupMin();
				}
			);
			app.UseRouting();
			app.UseCors(CorsPolicies.AllowDatatables);
			app.UseAuthorization();
			app.Use(async (context, next) =>
			{
				context.Response.OnStarting((state) =>
				{
					var lang = "en";
					var ctx = (HttpContext)state;
					if (ctx.Request.Query.ContainsKey("lang"))
					{
						var allowedLangs = new[] { "en", "fr", "nl" };
						var queryLang = ctx.Request.Query["lang"].FirstOrDefault();
						if (allowedLangs.Contains(queryLang))
						{
							lang = queryLang;
						}
					}
					ctx.Response.Headers.ContentLanguage = lang;
					return Task.CompletedTask;
				}, context);

				await next();
			});

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapChangePassword(() => spaRouteService.GenerateUrl("account-profile", new { }));
				endpoints.MapOpenSearch();
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller}/{action=Index}/{id?}")
				.RequireCors(CorsPolicies.AllowDatatables);

				endpoints.MapDefaultSitemapXmlStylesheet();
			});

			app.UseResponseCaching();
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
					//}
				});
			}

			app.UseWhen(
				context => ShouldUseSpa(context),
				app2 => app2
					.UseSpaImproved(spa =>
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
						//	}
						//};

						spa.UseSpaPrerendering(options =>
						{
#if RebuildSPA
							options.BootModuleBuilder = env.IsDevelopment() ? new AngularCliBuilder(npmScript: "build:ssr") : null;
#else
							options.BootModuleBuilder = null;
#endif
							options.BootModulePath = $"{spa.Options.SourcePath}/dist/server/main.js";
							options.ExcludeUrls = new[] { "/sockjs-node" };
						});

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
