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
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUsernameAsync(string email);
    }

    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(IDbContext context) : base(context, "users")
        {

        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            FilterDefinition<User> filter = Builders<User>.Filter.Regex(u => u.Username, new MongoDB.Bson.BsonRegularExpression(username, "i"));
            return await _dbCollection.Find(filter).FirstOrDefaultAsync();
        }
    }
}
