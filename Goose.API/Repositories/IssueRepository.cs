using Goose.Data.Context;
using Goose.Data.Repository;
using Goose.Domain.Models.tickets;

namespace Goose.API.Repositories
{
    public interface IIssueRepository : IRepository<Issue>
    {
        
    }

    public class IssueRepository: Repository<Issue>, IIssueRepository
    {
        public IssueRepository(IDbContext context, string alternativCollectionName = null) : base(context, alternativCollectionName)
        {
        }
    }
}