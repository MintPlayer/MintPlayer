using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Data.Options;
using MintPlayerCrawler.Data.Extensions;
using MintPlayerCrawler.Data.Options;

namespace MintPlayerCrawler.Web
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
            services.AddMintPlayerCrawler(options =>
            {
                options.MintPlayerConnectionString = @"Server=(localdb)\mssqllocaldb;Database=MintPlayer;Trusted_Connection=True;ConnectRetryCount=0";
                options.ConnectionString = @"Server=(localdb)\mssqllocaldb;Database=MintPlayerCrawler;Trusted_Connection=True;ConnectRetryCount=0";
                options.JwtIssuerOptions = new MintPlayer.Data.Options.JwtIssuerOptions
                {
                    Issuer = Configuration["JwtIssuerOptions:Issuer"],
                    Audience = Configuration["JwtIssuerOptions:Audience"],
                    Subject = Configuration["JwtIssuerOptions:Subject"],
                    ValidFor = Configuration.GetValue<TimeSpan>("JwtIssuerOptions:ValidFor"),
                    Key = Configuration["JwtIssuerOptions:Key"],
                };
            });

            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, Services.MintPlayerCrawlerService>();

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services
                .Configure<JwtIssuerOptions>(options =>
                {
                    options.Issuer = Configuration["JwtIssuerOptions:Issuer"];
                    options.Subject = Configuration["JwtIssuerOptions:Subject"];
                    options.Audience = Configuration["JwtIssuerOptions:Audience"];
                    options.ValidFor = Configuration.GetValue<TimeSpan>("JwtIssuerOptions:ValidFor");
                    options.Key = Configuration["JwtIssuerOptions:Key"];
                })
                .Configure<MintPlayerCrawlerOptions>(options =>
                {
                    options.DaemonName = "MintPlayer Crawler";
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
                .UseStaticFiles()
                .UseSpaStaticFiles();

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

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
