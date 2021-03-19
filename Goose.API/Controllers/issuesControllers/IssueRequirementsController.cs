using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services.issues;
using Goose.Domain.DTOs.issues;
using Microsoft.AspNetCore.Mvc;

namespace Goose.API.Controllers.issuesControllers
{
    [Route("/api/issues/")]
    [ApiController]
    public class IssueRequirementsController : Controller
    {
        private readonly IIssueRequirementService _issueService;

        public IssueRequirementsController(IIssueRequirementService issueRepo)
        {
            _issueService = issueRepo;
        }

        [HttpGet]
        public async Task<ActionResult<IList<IssueRequirementDTO>>> GetAll([FromRoute] string issueId)
        {
            return Ok(await _issueService.GetAllOfIssueAsync(issueId.ToObjectId()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IssueRequirementDTO>> Get([FromRoute] string issueId, [FromRoute] string id)
        {
            return Ok(await _issueService.GetAsync(issueId.ToObjectId(), id.ToObjectId()));
        }

        [HttpPost]
        public async Task<ActionResult<IList<IssueRequirementDTO>>> Post([FromRoute] string issueId,
            [FromBody] IssueRequirementDTO requirement)
        {
            var res = await _issueService.CreateAsync(issueId.ToObjectId(), requirement);
            return CreatedAtAction(nameof(Get), new {id = res.Id}, res);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<IList<IssueRequirementDTO>>> Put([FromRoute] string id,
            [FromBody] IssueRequirementDTO requirement)
        {
            await _issueService.UpdateAsync(id.ToObjectId(), requirement);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<IList<IssueRequirementDTO>>> Delete([FromRoute] string id)
        {
            await _issueService.DeleteAsync(id.ToObjectId());
            return NoContent();
        }
    }
}