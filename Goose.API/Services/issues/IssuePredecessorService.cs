using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Repositories;
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

        public IssuePredecessorService(IIssueRepository issueRepo)
        {
            _issueRepo = issueRepo;
        }

        public async Task SetPredecessor(ObjectId projectId, ObjectId successorId, ObjectId predecessorId)
        {
            var successor = await _issueRepo.GetOfProjectAsync(projectId, successorId);
            var predecessor = await _issueRepo.GetOfProjectAsync(projectId, predecessorId);

            //TODO checken ob es eine dealock gäbe
            successor.PredecessorIssueIds.Add(predecessorId);
            predecessor.SuccessorIssueIds.Add(successorId);

            await Task.WhenAll(_issueRepo.UpdateAsync(successor), _issueRepo.UpdateAsync(predecessor));
        }

        public async Task RemovePredecessor(ObjectId projectId, ObjectId successorId, ObjectId predecessorId)
        {
            var successor = await _issueRepo.GetOfProjectAsync(projectId, successorId);
            var predecessor = await _issueRepo.GetOfProjectAsync(projectId, predecessorId);
            if (successor.PredecessorIssueIds.Remove(predecessorId) ||
                predecessor.SuccessorIssueIds.Remove(successorId))
                await Task.WhenAll(_issueRepo.UpdateAsync(successor), _issueRepo.UpdateAsync(predecessor));
        }
    }
}