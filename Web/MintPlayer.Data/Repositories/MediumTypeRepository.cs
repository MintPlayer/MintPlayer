using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MintPlayer.Data.Repositories.Interfaces;

namespace MintPlayer.Data.Repositories
{
    internal class MediumTypeRepository : IMediumTypeRepository
    {
        private IHttpContextAccessor http_context;
        private MintPlayerContext mintplayer_context;
        private UserManager<Entities.User> user_manager;
        public MediumTypeRepository(IHttpContextAccessor http_context, MintPlayerContext mintplayer_context, UserManager<Entities.User> user_manager)
        {
            this.http_context = http_context;
            this.mintplayer_context = mintplayer_context;
            this.user_manager = user_manager;
        }

        public IEnumerable<Dtos.MediumType> GetMediumTypes(bool include_relations = false)
        {
            var medium_types = mintplayer_context.MediumTypes
                .Select(mt => ToDto(mt));
            return medium_types;
        }

        public Dtos.MediumType GetMediumType(int id, bool include_relations = false)
        {
            var medium_type = mintplayer_context.MediumTypes
                .SingleOrDefault(mt => mt.Id == id);
            return ToDto(medium_type);
        }

        public async Task<Dtos.MediumType> InsertMediumType(Dtos.MediumType mediumType)
        {
            var entity_medium_type = ToEntity(mediumType, mintplayer_context);

            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            entity_medium_type.UserInsert = user;

            mintplayer_context.MediumTypes.Add(entity_medium_type);
            await mintplayer_context.SaveChangesAsync();

            return ToDto(entity_medium_type);
        }

        public async Task<Dtos.MediumType> UpdateMediumType(Dtos.MediumType mediumType)
        {
            var entity_medium_type = mintplayer_context.MediumTypes
                .SingleOrDefault(mt => mt.Id == mediumType.Id);

            entity_medium_type.Description = mediumType.Description;
            entity_medium_type.PlayerType = mediumType.PlayerType;

            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            entity_medium_type.UserUpdate = user;

            return ToDto(entity_medium_type);
        }

        public async Task DeleteMediumType(int medium_type_id)
        {
            var medium_type = mintplayer_context.MediumTypes.Find(medium_type_id);
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            medium_type.UserDelete = user;
        }

        public async Task SaveChangesAsync()
        {
            await mintplayer_context.SaveChangesAsync();
        }

        #region Conversion Methods
        internal static Dtos.MediumType ToDto(Entities.MediumType mediumType)
        {
            if (mediumType == null) return null;
            return new Dtos.MediumType
            {
                Id = mediumType.Id,
                Description = mediumType.Description,
                PlayerType = mediumType.PlayerType
            };
        }
        internal static Entities.MediumType ToEntity(Dtos.MediumType mediumType, MintPlayerContext mintplayer_context)
        {
            if (mediumType == null) return null;
            return new Entities.MediumType
            {
                Id = mediumType.Id,
                Description = mediumType.Description,
                PlayerType = mediumType.PlayerType
            };
        }
        #endregion
    }
}
