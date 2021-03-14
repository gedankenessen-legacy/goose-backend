using Goose.Data.Context;
using Goose.Data.Repository;
using Goose.Domain.DTOs;
using Goose.Domain.Models;
using Goose.Domain.Models.projects;
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
        Task<UpdateResult> UpdateProject(ObjectId projectId, string name);
        Task<UpdateResult> AddState(ObjectId projectId, State state);
        Task UpdateState(ObjectId projectId, ObjectId stateId, string name, string phase);
    }

    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        public ProjectRepository(IDbContext context) : base(context, "projects")
        {

        }

        public Task<UpdateResult> AddState(ObjectId projectId, State state)
        {
            var update = Builders<Project>.Update.Push(x => x.States, state);
            return UpdateByIdAsync(projectId, update);
        }

        public Task<UpdateResult> UpdateProject(ObjectId projectId, string name)
        {
            var update = Builders<Project>.Update.Set(x => x.ProjectDetail.Name, name);
            return UpdateByIdAsync(projectId, update);
        }

        public Task UpdateState(ObjectId projectId, ObjectId stateId, string name, string phase)
        {
            var update = Builders<Project>.Update
                .Set("states.$[matches].name", name)
                .Set("states.$[matches].phase", phase)
                .Set("states.$[matches].updatedAt", DateTime.Now);

            ArrayFilterDefinition<BsonDocument> filter = new BsonDocument("matches._id", stateId);

            var options = new UpdateOptions()
            {
                ArrayFilters = new [] {filter},
            };
            return UpdateByIdAsync(projectId, update, options);
        }
    }
}
