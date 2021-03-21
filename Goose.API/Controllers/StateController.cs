using Goose.API.Services;
using Goose.Data;
using Goose.Domain.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Controllers
{
    [Route("api/project/{projectId}/state")]

    public class StateController : ControllerBase
    {
        private readonly IStateService _stateService;

        public StateController(IStateService stateService)
        {
            _stateService = stateService;
        }

        // POST: api/project/{projectId}/state/
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<StateDTO>> CreateState([FromBody] StateDTO stateDTO, [FromRoute] string projectId)
        {
            var state = await _stateService.CreateStateAsync(ObjectIdConverter.Validate(projectId), stateDTO);
            return Ok(state);
        }

        // PUT: api/project/{projectId}/state/{stateId}
        [HttpPut("{stateId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateState([FromBody] StateDTO stateDTO, [FromRoute] string projectId, string stateId)
        {
            await _stateService.UpdateState(ObjectIdConverter.Validate(projectId), ObjectIdConverter.Validate(stateId), stateDTO);
            return NoContent();
        }

        // GET: api/project/{projectId}/state
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IList<StateDTO>>> GetStates([FromRoute] string projectId)
        {
            var states = await _stateService.GetStates(ObjectIdConverter.Validate(projectId));
            return Ok(states);
        }

        // GET: api/project/{projectId}/state/{stateId}
        [HttpGet("{stateId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StateDTO>> GetState([FromRoute] string projectId, string stateId)
        {
            var state = await _stateService.GetState(ObjectIdConverter.Validate(projectId), ObjectIdConverter.Validate(stateId));
            if (state != null)
            {
                return Ok(state);
            }
            else
            {
                return NotFound();
            }
        }

        // Delete: pi/project/{projectId}/state/{stateId}
        [HttpDelete("{stateId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteState([FromRoute] string projectId, string stateId)
        {
            await _stateService.DeleteState(ObjectIdConverter.Validate(projectId), ObjectIdConverter.Validate(stateId));
            return NoContent();
        }
    }
}
