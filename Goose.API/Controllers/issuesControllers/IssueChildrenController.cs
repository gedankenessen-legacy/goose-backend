using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services.issues;
using Goose.Domain.DTOs.Issues;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Goose.API.Controllers.issuesControllers
{
    public interface IIssueChildrenController
    {
        public Task<IList<IssueDTO>> GetChildren(ObjectId parentId, bool recursive = false);
    }

    [Route("api/issues/{parentId}/children")]
    [ApiController]
    public class IssueChildrenController: IIssueChildrenController
    {
        private readonly IIssueChildrenService _childrenService;

        public IssueChildrenController(IIssueChildrenService childrenService)
        {
            _childrenService = childrenService;
        }

        [HttpGet]
        public async Task<IList<IssueDTO>> GetChildren(ObjectId parentId, [FromQuery] bool recursive = false) => await _childrenService.GetAll(parentId, recursive);
    }
}