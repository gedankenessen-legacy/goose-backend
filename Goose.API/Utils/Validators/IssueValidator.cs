using System.Threading.Tasks;
using Goose.API.Repositories;
using MongoDB.Bson;

namespace Goose.API.Utils.Validators
{
    public interface IIssueRequestValidator
    {
        public Task<bool> HasExistingProjectId(ObjectId projectId);
    }
    public class IssueRequestValidator: IIssueRequestValidator
    {
        private readonly IProjectRepository _projectRepository;

        public IssueRequestValidator(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<bool> HasExistingProjectId(ObjectId projectId)
        {
            return await _projectRepository.GetAsync(projectId) != null;
        }
    }
}