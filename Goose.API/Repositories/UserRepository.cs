﻿using Goose.Data.Context;
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
        Task<User?> GetByUsernameAsync(string username);
    }

    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(IDbContext context) : base(context, "users")
        {

        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbCollection.Find(usr => usr.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();
        }
    }
}
