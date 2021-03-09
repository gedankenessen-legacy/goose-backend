using Goose.Data.Context;
using Goose.Data.Repository;
using Goose.Domain.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Repositories
{
    public interface IProjectRepository : IRepository<Project>
    {
        Task<UpdateResult> UpdateProject(string id, string name, string companyId);
    }

    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        public ProjectRepository(IDbContext context) : base(context, "projects")
        {

        }

        public Task<UpdateResult> UpdateProject(string id, string name, string companyId)
        {
            var update = Builders<Project>.Update.Set(x => x.Details.Name, name).Set(x => x.CompanyId, companyId);
            return _dbCollection.UpdateOneAsync(x => x.Id == id, update);
        }
    }
}
