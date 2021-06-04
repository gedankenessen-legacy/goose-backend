using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Authorization;
using Goose.API.Authorization.Requirements;
using Goose.API.Repositories;
using Goose.API.Services.Issues;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Issues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace Goose.API.Services.issues
{
    public interface IIssueChildrenService
    {
        public Task<IList<IssueDTO>> GetAll(ObjectId issueId, bool recursive);
    }

    public class IssueChildrenService : IIssueChildrenService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IIssueService _issueService;
        private readonly IIssueHelper _issueHelper;
        private readonly IIssueRepository _issueRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;


        public IssueChildrenService(IIssueService issueService, IIssueRepository issueRepository, IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor, IProjectRepository projectRepository, IIssueHelper issueHelper)
        {
            _issueService = issueService;
            _issueRepository = issueRepository;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _projectRepository = projectRepository;
            _issueHelper = issueHelper;
        }

        public async Task<IList<IssueDTO>> GetAll(ObjectId issueId, bool recursive)
        {
            var issue = await _issueRepository.GetAsync(issueId);
            if (issue == null) throw new HttpStatusException(StatusCodes.Status400BadRequest, $"Issue {issueId} does not exist");

            //TODO Field can be null because DB was not cleared yet
            if (issue.ChildrenIssueIds == null) return new List<IssueDTO>();

            IList<Issue> children;
            if (recursive) children = await _issueHelper.GetChildrenRecursive(issue);
            else children = await Task.WhenAll(issue.ChildrenIssueIds.Select(it => _issueRepository.GetAsync(it)));
            if (await _authorizationService.HasAtLeastOneRequirement(_httpContextAccessor.HttpContext.User,
                await _projectRepository.GetAsync(issue.ProjectId), ProjectRolesRequirement.CustomerRequirement))
                children = children.Where(it => it.IssueDetail.Visibility).ToList();

            return await Task.WhenAll(children.Select(it => _issueService.Get(it.Id)));
        }
    }
}