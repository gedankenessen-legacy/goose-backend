using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services;
using Goose.Domain.DTOs.issues;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Goose.API.Controllers
{
    [Route("api/projects/{projectId}/issues/")]
    [ApiController]
    public class IssuesController : Controller
    {
        private readonly IIssueService _issueService;

        public IssuesController(IIssueService issueService)
        {
            _issueService = issueService;
        }


        [HttpGet]
        public async Task<ActionResult<IList<IssueDTO>>> GetAll([FromRoute] string projectId)
        {
            var res = await _issueService.GetAllOfProject(new ObjectId(projectId));
            return ActionResultOk(res);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IssueDTO>> Get([FromRoute] string projectId, [FromRoute] string id)
        { 
            var res = await _issueService.GetOfProject(new ObjectId(projectId), new ObjectId(id));
            return res == null ? NotFound() : ActionResultOk(res);
        }

        [HttpPost]
        public async Task<ActionResult<IssueDTO>> Post([FromBody] IssueDTO issue, [FromRoute] string projectId)
        {
            var res = await _issueService.Create(issue);
            return ActionResultCreated($"api/projects/{projectId}/issues/{res.Id}", res);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<IssueDTO>> Put([FromBody] IssueDTO issue, [FromRoute] string projectId,
            [FromRoute] string id)
        {
            if (!new ObjectId(id).Equals(issue.Id)) return NotFound();
            var result = await Get(projectId, id);
            if (result == null) BadRequest();

            await _issueService.Update(issue);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<IssueDTO>> Delete([FromRoute] string projectId, [FromRoute] string id)
        {
            var result = await Get(projectId, id);
            if (result == null) BadRequest();

            await _issueService.Delete(new ObjectId(id));

            return NoContent();
        }

        private ActionResult<T> ActionResultOk<T>(T e)
        {
            return Ok(e);
        }

        private ActionResult<T> ActionResultCreated<T>(string url, T e)
        {
            return Created(url, e);
        }
    }
}