using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.Domain.DTOs.Issues;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Goose.API.Controllers.issuesControllers
{
    [Route("api/users/{userId}/issues/")]
    [ApiController]
    public class UserIssuesController
    {
        [HttpGet]
        public async Task<ActionResult<IList<IssueDTO>>> GetAllOfUser(ObjectId userId)
        {
            return null;
        }
    }
}