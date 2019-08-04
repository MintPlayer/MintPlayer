using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MintPlayer.Data.Options;
using MintPlayer.Data.Repositories.Interfaces;
using MintPlayer.Data.Repositories;
using MintPlayer.Data.Helpers;

namespace MintPlayer.Data.Extensions
{
    public static class MintPlayer
    {
        public static IServiceCollection AddMintPlayer(this IServiceCollection services, Action<MintPlayerOptions> options)
        {
            var opt = new MintPlayerOptions();
            options.Invoke(opt);

            services.AddDbContext<MintPlayerContext>(db_options =>
            {
                db_options.UseSqlServer(opt.ConnectionString, b => b.MigrationsAssembly("MintPlayer.Data"));
            });

            services.AddIdentity<Entities.User, Entities.Role>()
                .AddEntityFrameworkStores<MintPlayerContext>()
                .AddDefaultTokenProviders();

            services
                .AddScoped<IAccountRepository, AccountRepository>()
                .AddScoped<IRoleRepository, RoleRepository>()
                .AddScoped<IPersonRepository, PersonRepository>()
                .AddScoped<IArtistRepository, ArtistRepository>()
                .AddScoped<ISongRepository, SongRepository>()
                .AddTransient<ArtistHelper>()
                .AddTransient<SongHelper>();

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

            services.AddDataProtection();

            services
                .Configure<JwtIssuerOptions>(jwt_options =>
                {
                    jwt_options.Audience = opt.JwtIssuerOptions.Audience;
                    jwt_options.Issuer = opt.JwtIssuerOptions.Issuer;
                    jwt_options.Key = opt.JwtIssuerOptions.Key;
                    jwt_options.Subject = opt.JwtIssuerOptions.Subject;
                    jwt_options.ValidFor = opt.JwtIssuerOptions.ValidFor;
                });

            return services;
        }
    }
}
