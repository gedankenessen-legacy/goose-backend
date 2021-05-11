using Goose.API.Services;
using Goose.API.Utils.Validators;
using Goose.Data;
using Goose.Domain.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Controllers
{
    [Route("api/projects/{projectId}/states")]
    [ApiController]
    public class StateController : ControllerBase
    {
        private readonly IStateService _stateService;

        public StateController(IStateService stateService)
        {
            _stateService = stateService;
        }

        // POST: api/projects/{projectId}/states/
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<StateDTO>> CreateState([FromBody] StateDTO stateDTO, [FromRoute] ObjectId projectId)
        {
            var state = await _stateService.CreateStateAsync(projectId, stateDTO);
            return Ok(state);
        }

        // PUT: api/projects/{projectId}/states/{stateId}
        [HttpPut("{stateId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateState([FromBody] StateDTO stateDTO, [FromRoute] ObjectId projectId, ObjectId stateId)
        {
            await _stateService.UpdateState(projectId, stateId, stateDTO);
            return NoContent();
        }

        // GET: api/projects/{projectId}/states
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IList<StateDTO>>> GetStates([FromRoute] ObjectId projectId)
        {
            var states = await _stateService.GetStates(projectId);
            return Ok(states);
        }

        // GET: api/projects/{projectId}/states/{stateId}
        [HttpGet("{stateId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StateDTO>> GetState([FromRoute] ObjectId projectId, ObjectId stateId)
        {
            var state = await _stateService.GetState(projectId, stateId);
            if (state != null)
            {
                return Ok(state);
            }
            else
            {
                return NotFound();
            }
        }

        // Delete: api/projects/{projectId}/states/{stateId}
        [HttpDelete("{stateId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteState([FromRoute] ObjectId projectId, ObjectId stateId)
        {
            await _stateService.DeleteState(projectId, stateId);
            return NoContent();
        }
    }
}
