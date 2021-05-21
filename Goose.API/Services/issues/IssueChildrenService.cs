using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Authorization;
using Goose.API.Authorization.Requirements;
using Goose.API.Repositories;
using Goose.API.Services.Issues;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs.Issues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace Goose.API.Services.issues
{
    public interface IIssueChildrenService
    {
        public Task<IList<IssueDTO>> GetAll(ObjectId issueId);
    }

    public class IssueChildrenService : IIssueChildrenService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IIssueService _issueService;
        private readonly IIssueRepository _issueRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;


        public IssueChildrenService(IIssueService issueService, IIssueRepository issueRepository, IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor, IProjectRepository projectRepository)
        {
            _issueService = issueService;
            _issueRepository = issueRepository;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _projectRepository = projectRepository;
        }

        public async Task<IList<IssueDTO>> GetAll(ObjectId issueId)
        {
            var issue = await _issueRepository.GetAsync(issueId);
            if (issue == null) throw new HttpStatusException(StatusCodes.Status400BadRequest, $"Issue {issueId} does not exist");
            
            //TODO Field can be null because DB was not cleared yet
            if (issue.ChildrenIssueIds == null) return new List<IssueDTO>();
            
            var children = await Task.WhenAll(issue.ChildrenIssueIds.Select(it => _issueRepository.GetAsync(it)));
            if (await _authorizationService.HasAtLeastOneRequirement(_httpContextAccessor.HttpContext.User,
                await _projectRepository.GetAsync(issue.ProjectId), ProjectRolesRequirement.CustomerRequirement))
                children = children.Where(it => it.IssueDetail.Visibility).ToArray();
            
            return await Task.WhenAll(children.Select(it => _issueService.Get(it.Id)));
        }
    }
}