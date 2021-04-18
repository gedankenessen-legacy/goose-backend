using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services.Issues;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Tickets;
using Goose.API.Utils;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Goose.API.Controllers.IssuesControllers
{
    [Route("/api/issues/{issueId}/requirements/")]
    [ApiController]
    public class IssueRequirementsController : Controller
    {
        private readonly IIssueRequirementService _issueRequirementService;

        public IssueRequirementsController(IIssueRequirementService issueReqService)
        {
            _issueRequirementService = issueReqService;
        }

        [HttpGet]
        public async Task<ActionResult<IList<IssueRequirement>>> GetAll([FromRoute] ObjectId issueId)
        {
            return Ok(await _issueRequirementService.GetAllOfIssueAsync(issueId));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IssueRequirement>> Get([FromRoute] ObjectId issueId, [FromRoute] ObjectId id)
        {
            return Ok(await _issueRequirementService.GetAsync(issueId, id));
        }

        [HttpPost]
        public async Task<ActionResult<IList<IssueRequirement>>> Post([FromRoute] ObjectId issueId,
            [FromBody] IssueRequirement requirement)
        {
            var res = await _issueRequirementService.CreateAsync(issueId, requirement);
            return CreatedAtAction(nameof(Get), new {id = res.Id}, res);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<IList<IssueRequirement>>> Put([FromRoute] ObjectId id,
            [FromBody] IssueRequirement requirement)
        {
            await _issueRequirementService.UpdateAsync(id, requirement);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<IList<IssueRequirement>>> Delete([FromRoute] ObjectId issueId,
            [FromRoute] ObjectId id)
        {
            await _issueRequirementService.DeleteAsync(issueId, id);
            return NoContent();
        }
    }
}