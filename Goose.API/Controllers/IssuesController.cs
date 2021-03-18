using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services;
using Goose.Domain.DTOs.issues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Goose.API.Controllers
{
    [Route("api/issues/")]
    [ApiController]
    public class IssuesController : Controller
    {
        //TODO user auth
        //TODO verify issue is in project
        //TODO untertickets
        //TODO timesheets controller
        //TODO assigned controller
        private readonly IIssueService _issueService;

        public IssuesController(IIssueService issueService)
        {
            _issueService = issueService;
        }

        //api/issues/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IList<IssueResponseDTO>>> GetAll()
        {
            //var res = await _issueService.GetAllOfProject(new ObjectId(projectId));
            var res = await _issueService.GetAll();
            return Ok(res);
        }

        //api/issues/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IssueResponseDTO>> Get([FromRoute] string id)
        {
            var res = await _issueService.Get(id.ToObjectId());
            return res == null ? NotFound() : Ok(res);
        }

        //api/issues
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IssueResponseDTO>> Post([FromBody] IssueRequestDTO issueRequest)
        {
            var res = await _issueService.Create(issueRequest);
            return Created($"api/issues/{res.Id}", res);
        }

        //api/issues/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Put([FromBody] IssueRequestDTO issueRequest, [FromRoute] string id)
        {
            await _issueService.Update(issueRequest, id.ToObjectId());
            return NoContent();
        }

        //api/issues/{id}  
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Delete([FromRoute] string id)
        {
            if (await _issueService.Delete(id.ToObjectId()))
                return NoContent();
            return BadRequest();
        }
    }
}