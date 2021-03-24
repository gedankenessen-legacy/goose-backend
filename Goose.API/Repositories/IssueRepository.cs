using Goose.API.Utils.Exceptions;
using Goose.API.Utils.Validators;
using Goose.Data.Context;
using Goose.Data.Repository;
using Goose.Domain.Models.tickets;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Goose.API.Repositories
{
    public interface IIssueRepository : IRepository<Issue>
    {
        Task<IList<Issue>> GetAllOfProjectAsync(ObjectId projectId);
        Task<Issue> GetOfProjectAsync(ObjectId projectId, ObjectId issueId);
        public Task<Issue> GetIssueByIdAsync(string issueId);
        public Task CreateOrUpdateConversationItemAsync(string issueId, IssueConversation issueConversation);
        public Task<IssueRequirement> GetRequirementByIdAsync(ObjectId issueId, ObjectId requirementId);
    }

    public class IssueRepository : Repository<Issue>, IIssueRepository
    {
    
        public IssueRepository(IDbContext context) : base(context, "issues") { }
    
        public async Task<IList<Issue>> GetAllOfProjectAsync(ObjectId projectId)
        {
            return await FilterByAsync((it) => it.ProjectId.Equals(projectId));
        }

        public async Task<Issue> GetOfProjectAsync(ObjectId projectId, ObjectId issueId)
        {
            var res = await FilterByAsync((it) => it.ProjectId.Equals(projectId) && it.Id.Equals(issueId));
            return res.FirstOrDefault();
        }
        
        /// <summary>
        /// Returns a issue based on the provided issueId.
        /// </summary>
        /// <param name="issueId"></param>
        /// <returns>A issue for the provided issue id.</returns>
        public async Task<Issue> GetIssueByIdAsync(string issueId)
        {
            // check if the parsed objectId is not the 000...000 default objectId.
            ObjectId issueOid = Validators.ValidateObjectId(issueId, "Cannot parse issue string id to a valid object id.");


            // fetch the issue that contains the conversation.
            Issue issue = await GetAsync(issueOid);

            // show error if issue is not found.
            if (issue is null)
                throw new HttpStatusException(StatusCodes.Status404NotFound, $"No Issue found with Id={ issueId }.");

            return issue;
        }

        /// <summary>
        /// Creates a conversation item or updates it if existing.
        /// </summary>
        /// <param name="issueId">The issue on which the conversation will be added to.</param>
        /// <param name="issueConversation">The conversation item.</param>
        public async Task CreateOrUpdateConversationItemAsync(string issueId, IssueConversation issueConversation)
        {
            // check if the parsed objectId is not the 000...000 default objectId.
            ObjectId issueOid = Validators.ValidateObjectId(issueId, "Cannot parse issue string id to a valid object id.");

            var filterDef = Builders<Issue>.Filter;

            var issueFilter = filterDef.Eq(iss => iss.Id, issueOid);
            var distinguishFilter = filterDef.ElemMatch(iss => iss.ConversationItems, (ci) => ci.Id.Equals(issueConversation.Id));
            var push = Builders<Issue>.Update.Push(iss => iss.ConversationItems, issueConversation);

            var result = await _dbCollection.UpdateOneAsync(issueFilter & !distinguishFilter, push);

            // MatchedCount == 0 => filter does not return a conversation item, so we need to update one. If a ci was inserted the MatchedCount would be 1.
            if (result.MatchedCount == 0)
            {
                // [-1] actes as $-Operator from mongodb.
                var update = Builders<Issue>.Update.Set(iss => iss.ConversationItems[-1], issueConversation);
                await _dbCollection.UpdateOneAsync(issueFilter & distinguishFilter, update);
            }
        }

        public async Task<IssueRequirement> GetRequirementByIdAsync(ObjectId issueId, ObjectId requirementId)
        {
            // fetch the issue that contains the conversation.
            Issue issue = await GetAsync(issueId);

            // show error if issue is not found.
            if (issue is null)
                throw new HttpStatusException(StatusCodes.Status404NotFound, $"No Issue found with Id={issueId}.");

            var req = issue.IssueDetail.Requirements.SingleOrDefault(req => req.Id.Equals(requirementId));

            if (req is null)
                throw new HttpStatusException(StatusCodes.Status404NotFound, $"No Requirement found with Id={requirementId}.");

            return req;
        }
    }
}
