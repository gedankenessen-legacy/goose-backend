using Goose.Data.Context;
using Goose.Data.Repository;
using Goose.Domain.Models.Identity;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace Goose.API.Repositories
{

    public interface IRoleRepository : IRepository<Role>
    {
        public Task<Role?> GetFirstRoleByNameAsync(string name);
    }

    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        public RoleRepository(IDbContext context) : base(context, "roles")
        {

        }

        public async Task<Role?> GetFirstRoleByNameAsync(string name)
        {
            return (await FilterByAsync(role => role.Name.ToLower() == name.ToLower())).FirstOrDefault();
        }
    }
}
