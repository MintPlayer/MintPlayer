using System;

namespace MintPlayer.Data.Mappers
{
    internal interface IRoleMapper
    {
        MintPlayer.Dtos.Dtos.Role Entity2Dto(Entities.Role role);
        Entities.Role Dto2Entity(MintPlayer.Dtos.Dtos.Role role);
    }

    internal class RoleMapper : IRoleMapper
    {
        private readonly IServiceProvider serviceProvider;
        public RoleMapper(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public MintPlayer.Dtos.Dtos.Role Entity2Dto(Entities.Role role)
        {
            if (role == null)
            {
                return null;
            }

            return new MintPlayer.Dtos.Dtos.Role
            {
                Id = role.Id,
                Name = role.Name
            };
        }

        public Entities.Role Dto2Entity(MintPlayer.Dtos.Dtos.Role role)
        {
            if (role == null)
            {
                return null;
            }

            return new Entities.Role
            {
                Id = role.Id,
                Name = role.Name,
                NormalizedName = role.Name.Normalize()
            };
        }
    }
}
