using Goose.Data.Context;
using Goose.Data.Repository;
using Goose.Domain.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Repositories
{

    public interface IRoleRepository : IRepository<Role>
    {

    }

    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        public RoleRepository(IDbContext context) : base(context, "roles")
        {

        }
    }
}
