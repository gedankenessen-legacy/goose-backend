using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services.issues;
using Goose.Domain.DTOs.Issues;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Goose.API.Controllers.issuesControllers
{
    [Route("api/users/{userId}/projects/{projectId}/issues/")]
    [ApiController]
    public class UserIssuesController: Controller
    {
        private readonly IUserIssueService _userIssueService;

        public UserIssuesController(IUserIssueService userIssueService)
        {
            _userIssueService = userIssueService;
        }

        [HttpGet]
        public async Task<ActionResult<IList<IssueDTO>>> GetAllOfUser(ObjectId projectId, ObjectId userId)
        {
            var res = await _userIssueService.GetAllOfUser(projectId, userId);
            return Ok(res);
        }
    }
}