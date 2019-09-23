using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MintPlayer.Data;
using MintPlayerCrawler.Data.Options;
using MintPlayerCrawler.Data.Repositories;
using MintPlayerCrawler.Data.Repositories.Interfaces;

namespace MintPlayerCrawler.Data.Extensions
{
    public static class MintPlayerCrawler
    {
        public static IServiceCollection AddMintPlayerCrawler(this IServiceCollection services, Action<MintPlayerCrawlerSettings> options)
        {
            var opt = new MintPlayerCrawlerSettings();
            options.Invoke(opt);

            services
                .AddDbContext<MintPlayerContext>(db_options =>
                {
                    db_options.UseSqlServer(opt.MintPlayerConnectionString, b => b.MigrationsAssembly("MintPlayer.Data"));
                })
                .AddDbContext<MintPlayerCrawlerContext>(db_options =>
                {
                    db_options.UseSqlServer(opt.ConnectionString, b => b.MigrationsAssembly("MintPlayerCrawler.Data"));
                });

            services.AddIdentity<MintPlayer.Data.Entities.User, MintPlayer.Data.Entities.Role>()
                .AddEntityFrameworkStores<MintPlayerContext>()
                .AddDefaultTokenProviders();

            services
                .AddScoped<ILinkRepository, LinkRepository>();

            services
                .AddAuthentication(auth_options =>
                {
                    auth_options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    auth_options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    auth_options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(jwt_options =>
                {
                    jwt_options.Audience = opt.JwtIssuerOptions.Audience;
                    jwt_options.ClaimsIssuer = opt.JwtIssuerOptions.Issuer;
                    jwt_options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidAudience = opt.JwtIssuerOptions.Audience,
                        ValidateIssuer = true,
                        ValidIssuer = opt.JwtIssuerOptions.Issuer,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new Func<SymmetricSecurityKey>(() =>
                        {
                            var key = opt.JwtIssuerOptions.Key;
                            var bytes = System.Text.Encoding.UTF8.GetBytes(key);
                            var signing_key = new SymmetricSecurityKey(bytes);
                            return signing_key;
                        }).Invoke(),
                        ValidateLifetime = true
                    };
                    jwt_options.SaveToken = false;
                });

            return services;
        }
    }
}
