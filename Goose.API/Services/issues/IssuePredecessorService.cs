using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Repositories;
using Goose.API.Utils.Authentication;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Issues;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace Goose.API.Services.Issues
{
    public interface IIssuePredecessorService
    {
        public Task<IList<IssueDTO>> GetAll(ObjectId issueId);
        public Task SetPredecessor(ObjectId successorId, ObjectId predecessorId);
        public Task RemovePredecessor(ObjectId successorId, ObjectId predecessorId);
    }

    public class IssuePredecessorService : IIssuePredecessorService
    {
        //TODO ggf müssen vorgänger rekursiv entfernt werden?

        //TODO wie bekommt man am besten alle issues in einem vorgänger baum? phil fragen?
        private readonly IIssueRepository _issueRepo;
        private readonly IIssueService _issueService;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public IssuePredecessorService(IIssueRepository issueRepo, IIssueService issueService, IHttpContextAccessor httpContextAccessor)
        {
            _issueRepo = issueRepo;
            _httpContextAccessor = httpContextAccessor;
            _issueService = issueService;
        }

        public async Task<IList<IssueDTO>> GetAll(ObjectId issueId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            return await Task.WhenAll(issue.PredecessorIssueIds.Select(it => _issueService.Get(it)));
        }

        public async Task SetPredecessor(ObjectId successorId, ObjectId predecessorId)
        {
            var successor = await _issueRepo.GetAsync(successorId);
            var predecessor = await _issueRepo.GetAsync(predecessorId);

            if (successor.ProjectId != predecessor.ProjectId)
            {
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "Issues müssen im selben Projekt sein");
            }

            //TODO checken ob es eine dealock gäbe
            successor.PredecessorIssueIds.Add(predecessorId);
            predecessor.SuccessorIssueIds.Add(successorId);

            successor.ConversationItems.Add(new IssueConversation()
            {
                Id = ObjectId.GenerateNewId(),
                CreatorUserId = _httpContextAccessor.HttpContext.User.GetUserId(),
                Type = IssueConversation.PredecessorAddedType,
                Data = null,
                OtherTicketId = predecessorId,
            });

            await Task.WhenAll(_issueRepo.UpdateAsync(successor), _issueRepo.UpdateAsync(predecessor));
        }

        public async Task RemovePredecessor(ObjectId successorId, ObjectId predecessorId)
        {
            var successor = await _issueRepo.GetAsync(successorId);
            var predecessor = await _issueRepo.GetAsync(predecessorId);

            if (successor.PredecessorIssueIds.Remove(predecessorId) ||
                predecessor.SuccessorIssueIds.Remove(successorId))
            {
                successor.ConversationItems.Add(new IssueConversation()
                {
                    Id = ObjectId.GenerateNewId(),
                    CreatorUserId = _httpContextAccessor.HttpContext.User.GetUserId(),
                    Type = IssueConversation.PredecessorRemovedType,
                    Data = null,
                    OtherTicketId = predecessorId,
                });

                await Task.WhenAll(_issueRepo.UpdateAsync(successor), _issueRepo.UpdateAsync(predecessor));
            }
        }
    }
}