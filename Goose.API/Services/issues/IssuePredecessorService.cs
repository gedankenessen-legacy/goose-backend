using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Repositories;
using Goose.API.Utils.Authentication;
using Goose.Domain.Models.Issues;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace Goose.API.Services.Issues
{
    public interface IIssuePredecessorService
    {
        public Task SetPredecessor(ObjectId projectId, ObjectId successorId, ObjectId predecessorId);
        public Task RemovePredecessor(ObjectId projectId, ObjectId successorId, ObjectId predecessorId);
    }

    public class IssuePredecessorService : IIssuePredecessorService
    {
        //TODO ggf müssen vorgänger rekursiv entfernt werden?

        //TODO wie bekommt man am besten alle issues in einem vorgänger baum? phil fragen?
        private readonly IIssueRepository _issueRepo;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public IssuePredecessorService(IIssueRepository issueRepo, IHttpContextAccessor httpContextAccessor)
        {
            _issueRepo = issueRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task SetPredecessor(ObjectId projectId, ObjectId successorId, ObjectId predecessorId)
        {
            var successor = await _issueRepo.GetOfProjectAsync(projectId, successorId);
            var predecessor = await _issueRepo.GetOfProjectAsync(projectId, predecessorId);

            //TODO checken ob es eine dealock gäbe
            successor.PredecessorIssueIds.Add(predecessorId);
            predecessor.SuccessorIssueIds.Add(successorId);

            successor.ConversationItems.Add(new IssueConversation()
            {
                Id = ObjectId.GenerateNewId(),
                CreatorUserId = _httpContextAccessor.HttpContext.User.GetUserId(),
                Type = IssueConversation.PredecessorAddedType,
                Data = $"{predecessorId}",
            });

            await Task.WhenAll(_issueRepo.UpdateAsync(successor), _issueRepo.UpdateAsync(predecessor));
        }

        public async Task RemovePredecessor(ObjectId projectId, ObjectId successorId, ObjectId predecessorId)
        {
            var successor = await _issueRepo.GetOfProjectAsync(projectId, successorId);
            var predecessor = await _issueRepo.GetOfProjectAsync(projectId, predecessorId);
            if (successor.PredecessorIssueIds.Remove(predecessorId) ||
                predecessor.SuccessorIssueIds.Remove(successorId))
            {
                successor.ConversationItems.Add(new IssueConversation()
                {
                    Id = ObjectId.GenerateNewId(),
                    CreatorUserId = _httpContextAccessor.HttpContext.User.GetUserId(),
                    Type = IssueConversation.PredecessorRemovedType,
                    Data = $"{predecessorId}",
                });

                await Task.WhenAll(_issueRepo.UpdateAsync(successor), _issueRepo.UpdateAsync(predecessor));
            }
        }
    }
}