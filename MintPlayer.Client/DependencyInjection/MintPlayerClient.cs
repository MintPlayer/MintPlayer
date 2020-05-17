using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Client.Services;
using System.Net.Http;

namespace MintPlayer.Client.DependencyInjection
{
    public static class MintPlayerClient
    {
        public static IServiceCollection AddMintPlayerClient(this IServiceCollection services)
        {
            return services
                .AddSingleton<HttpClient>()
                .AddSingleton<IAccountService, AccountService>()
                .AddSingleton<IArtistService, ArtistService>()
                .AddSingleton<IMediumTypeService, MediumTypeService>()
                .AddSingleton<IPersonService, PersonService>()
                .AddSingleton<IPlaylistService, PlaylistService>()
                .AddSingleton<ISongService, SongService>()
                .AddSingleton<ISubjectService, SubjectService>()
                .AddSingleton<ITagCategoryService, TagCategoryService>()
                .AddSingleton<ITagService, TagService>();
        }
    }
}
