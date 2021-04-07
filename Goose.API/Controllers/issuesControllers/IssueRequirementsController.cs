using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services.Issues;
using Goose.API.Utils;
using Goose.Domain.Models.Tickets;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Goose.API.Controllers.IssuesControllers
{
    [Route("/api/issues/{issueId}/requirements/")]
    [ApiController]
    public class IssueRequirementsController : Controller
    {
        private readonly IIssueRequirementService _issueService;

        public IssueRequirementsController(IIssueRequirementService issueRepo)
        {
            _issueService = issueRepo;
        }

        [HttpGet]
        public async Task<ActionResult<IList<IssueRequirement>>> GetAll([FromRoute] ObjectId issueId)
        {
            return Ok(await _issueService.GetAllOfIssueAsync(issueId));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IssueRequirement>> Get([FromRoute] ObjectId issueId, [FromRoute] ObjectId id)
        {
            return Ok(await _issueService.GetAsync(issueId, id));
        }

        [HttpPost]
        public async Task<ActionResult<IList<IssueRequirement>>> Post([FromRoute] ObjectId issueId,
            [FromBody] IssueRequirement requirement)
        {
            var res = await _issueService.CreateAsync(issueId, requirement);
            return CreatedAtAction(nameof(Get), new {id = res.Id}, res);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<IList<IssueRequirement>>> Put([FromRoute] ObjectId id,
            [FromBody] IssueRequirement requirement)
        {
            await _issueService.UpdateAsync(id, requirement);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<IList<IssueRequirement>>> Delete([FromRoute] ObjectId issueId,
            [FromRoute] ObjectId id)
        {
            await _issueService.DeleteAsync(issueId, id);
            return NoContent();
        }
    }
}