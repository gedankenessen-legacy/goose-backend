using Goose.Data.Context;
using Goose.Data.Repository;
using Goose.Domain.Models.identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Repositories
{
    public interface IUserRepository : IRepository<User>
    {

    }

    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(IDbContext context) : base(context)
        {

        }
    }
}
