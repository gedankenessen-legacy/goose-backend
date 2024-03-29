﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services.Issues;
using Goose.API.Utils;
using Goose.Domain.DTOs.Issues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Goose.API.Controllers.IssuesControllers
{
    [Route("api/issues/{issueId}/timesheets/")]
    [ApiController]
    public class IssueTimeSheetsController : Controller
    {
        private readonly IIssueTimeSheetService _timeSheetService;

        public IssueTimeSheetsController(IIssueTimeSheetService timeSheetRepo)
        {
            _timeSheetService = timeSheetRepo;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<ActionResult<IList<IssueTimeSheetDTO>>> GetAll([FromRoute] ObjectId issueId)
        {
            return Ok(await _timeSheetService.GetAllOfIssueAsync(issueId));
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("{id}")]
        public async Task<ActionResult<IssueTimeSheetDTO>> Get([FromRoute] ObjectId issueId, [FromRoute] ObjectId id)
        {
            return Ok(await _timeSheetService.GetAsync(issueId, id));
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult<IssueTimeSheetDTO>> Post([FromRoute] ObjectId issueId,
            [FromBody] IssueTimeSheetDTO timeSheetDto)
        {
            var res = await _timeSheetService.CreateAsync(issueId, timeSheetDto);
            return CreatedAtAction(nameof(Get), new {issueId, id = res.Id}, res);
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put([FromRoute] ObjectId issueId, [FromRoute] ObjectId id,
            [FromBody] IssueTimeSheetDTO timeSheetDto)
        {
            await _timeSheetService.UpdateAsync(issueId, id, timeSheetDto);
            return NoContent();
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete("{id}")]
        public async Task<ActionResult<IList<IssueTimeSheetDTO>>> Delete([FromRoute] ObjectId issueId,
            [FromRoute] ObjectId id)
        {
            await _timeSheetService.DeleteAsync(issueId, id);
            return NoContent();
        }
    }
}