using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services.issues;
using Goose.Domain.DTOs.issues;
using Goose.Domain.Models.tickets;
using Microsoft.AspNetCore.Mvc;

namespace Goose.API.Controllers.issuesControllers
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
        public async Task<ActionResult<IList<IssueRequirement>>> GetAll([FromRoute] string issueId)
        {
            return Ok(await _issueService.GetAllOfIssueAsync(issueId.ToObjectId()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IssueRequirement>> Get([FromRoute] string issueId, [FromRoute] string id)
        {
            return Ok(await _issueService.GetAsync(issueId.ToObjectId(), id.ToObjectId()));
        }

        [HttpPost]
        public async Task<ActionResult<IList<IssueRequirement>>> Post([FromRoute] string issueId,
            [FromBody] IssueRequirement requirement)
        {
            var res = await _issueService.CreateAsync(issueId.ToObjectId(), requirement);
            return CreatedAtAction(nameof(Get), new {id = res.Id}, res);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<IList<IssueRequirement>>> Put([FromRoute] string id,
            [FromBody] IssueRequirement requirement)
        {
            await _issueService.UpdateAsync(id.ToObjectId(), requirement);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<IList<IssueRequirement>>> Delete([FromRoute] string issueId,
            [FromRoute] string id)
        {
            await _issueService.DeleteAsync(issueId.ToObjectId(), id.ToObjectId());
            return NoContent();
        }
    }
}