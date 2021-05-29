using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services.Issues;
using Goose.Domain.DTOs.Issues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Goose.API.Controllers.IssuesControllers
{
    [Route("api/issues/{issueId}/successors/")]
    [ApiController]
    public class IssueSuccessorsController
    {
        private readonly IIssueSuccessorService _issueService;

        public IssueSuccessorsController(IIssueSuccessorService issueService)
        {
            _issueService = issueService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IList<IssueDTO>> GetAll([FromRoute] ObjectId issueId)
        {
            return await _issueService.GetAll(issueId);
        }

/*
             * Issue wird blockiert wenn:
             * 1) Das Oberticket nicht in der Bearbeitungsphase ist*/
    }
}