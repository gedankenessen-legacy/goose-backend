using Goose.API.Services.issues;
using Goose.Domain.Models.Issues;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Controllers.issuesControllers
{
    [Route("/api/issues/{issueId}/summaries/")]
    [ApiController]
    public class IssueSummaryController : Controller
    {
        private readonly IIssueSummaryService _issueSummaryService;

        public IssueSummaryController(IIssueSummaryService issueSummaryService)
        {
            _issueSummaryService = issueSummaryService;
        }

        [HttpGet]
        public async Task<ActionResult<IList<IssueRequirement>>> GetSummary([FromRoute] string issueId)
        {
            var requirements = await _issueSummaryService.GetSummary(issueId);
            return Ok(requirements);
        }

        [HttpPost]
        public async Task<ActionResult<IList<IssueRequirement>>> CreateSummary([FromRoute] string issueId)
        {
            var requirements = await _issueSummaryService.CreateSummary(issueId);
            return Ok(requirements);
        }

        [HttpPut]
        public async Task<ActionResult> AcceptSummary([FromRoute] string issueId, [FromQuery] bool accept)
        {
            if(accept)
                await _issueSummaryService.AcceptSummary(issueId);
            else
                await _issueSummaryService.DeclineSummary(issueId);
            return Ok();
        }
    }
}
