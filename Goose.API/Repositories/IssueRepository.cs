using Goose.Data.Context;
using Goose.Data.Repository;
using Goose.Domain.Models.tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Repositories
{
    public interface IIssueRepository : IRepository<Issue>
    {

    }

    public class IssueRepository : Repository<Issue>, IIssueRepository
    {
        public IssueRepository(IDbContext context) : base(context, "issues")
        {
        }
    }
}
