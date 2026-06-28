using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MintPlayer.Data.Mappers;
using MintPlayer.Dtos.Dtos;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IRoleMapper roleMapper;
        public RoleRepository(
            RoleManager<Entities.Role> roleManager,
            IHttpContextAccessor httpContextAccessor,
            IRoleMapper roleMapper)
        {
            this.roleManager = roleManager;
            this.httpContextAccessor = httpContextAccessor;
            this.roleMapper = roleMapper;
        }

        public Task<IEnumerable<Role>> GetRoles()
        {
            return Task.FromResult<IEnumerable<Role>>(roleManager.Roles.Select(r => roleMapper.Entity2Dto(r)));
        }
    }
}
