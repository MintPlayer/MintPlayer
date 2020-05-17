using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MintPlayer.Dtos.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace MintPlayer.Data.Repositories
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetRoles();
    }
    internal class RoleRepository : IRoleRepository
    {
        private readonly RoleManager<Entities.Role> roleManager;
        private readonly IHttpContextAccessor httpContextAccessor;
        public RoleRepository(RoleManager<Entities.Role> roleManager, IHttpContextAccessor httpContextAccessor)
        {
            this.roleManager = roleManager;
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task<IEnumerable<Role>> GetRoles()
        {
            return Task.FromResult<IEnumerable<Role>>(roleManager.Roles.Select(r => ToDto(r)));
        }

        #region Conversion Methods
        internal static Entities.Role ToEntity(Role role)
        {
            if (role == null) return null;
            return new Entities.Role
            {
                Id = role.Id,
                Name = role.Name,
                NormalizedName = role.Name.Normalize()
            };
        }
        internal static Role ToDto(Entities.Role role)
        {
            if (role == null) return null;
            return new Role
            {
                Id = role.Id,
                Name = role.Name
            };
        }
        #endregion
    }
}
