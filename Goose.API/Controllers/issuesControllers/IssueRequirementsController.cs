using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services.Issues;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Issues;
using Goose.API.Utils;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<IList<IssueRequirement>>> GetAll([FromRoute] string issueId)
        {
            return Ok(await _issueRequirementService.GetAllOfIssueAsync(issueId.ToObjectId()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IssueRequirement>> Get([FromRoute] string issueId, [FromRoute] string id)
        {
            return Ok(await _issueRequirementService.GetAsync(issueId.ToObjectId(), id.ToObjectId()));
        }

        [HttpPost]
        public async Task<ActionResult<IList<IssueRequirement>>> Post([FromRoute] string issueId,
            [FromBody] IssueRequirement requirement)
        {
            var res = await _issueRequirementService.CreateAsync(issueId.ToObjectId(), requirement);
            return Ok(res);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<IList<IssueRequirement>>> Put([FromRoute] string id,
            [FromBody] IssueRequirement requirement)
        {
            await _issueRequirementService.UpdateAsync(id.ToObjectId(), requirement);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<IList<IssueRequirement>>> Delete([FromRoute] string issueId,
            [FromRoute] string id)
        {
            await _issueRequirementService.DeleteAsync(issueId.ToObjectId(), id.ToObjectId());
            return NoContent();
        }
    }
}