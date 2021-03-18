using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.Data.Context;
using Goose.Data.Repository;
using Goose.Domain.Models.tickets;
using MongoDB.Bson;

namespace Goose.API.Repositories
{
    public interface IIssueRepository : IRepository<Issue>
    {
        Task<IList<Issue>> GetAllOfProjectAsync(ObjectId projectId);
        Task<Issue> GetOfProjectAsync(ObjectId projectId, ObjectId issueId);
    }

    public class IssueRepository : Repository<Issue>, IIssueRepository
    {
        public IssueRepository(IDbContext context, string alternativCollectionName = "issues") : base(context,
            alternativCollectionName)
        {
        }

        public async Task<IList<Issue>> GetAllOfProjectAsync(ObjectId projectId)
        {
            return await FilterByAsync((it) => it.ProjectId.Equals(projectId));
        }

        public async Task<Issue> GetOfProjectAsync(ObjectId projectId, ObjectId issueId)
        {
            var res = await FilterByAsync((it) => it.ProjectId.Equals(projectId) && it.Id.Equals(issueId));
            return res.FirstOrDefault();
        }
    }
}