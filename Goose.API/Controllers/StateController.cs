using Goose.API.Services;
using Goose.Data;
using Goose.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Controllers
{
    [Route("api/company/{companyId}/project/{projectId}/state")]
    public class StateController : ControllerBase
    {
        private readonly IStateService _stateService;

        public StateController(IStateService stateService)
        {
            _stateService = stateService;
        }

        // POST: api/company/{companyId}/project/{projectId}/state/
        [HttpPost]
        public async Task<ActionResult<StateDTO>> CreateState([FromBody] StateDTO stateDTO, [FromRoute] string projectId)
        {
            var state = await _stateService.CreateStateAsync(ObjectIdConverter.Validate(projectId), stateDTO);
            return Ok(state);
        }

        // PUT: api/company/{companyId}/project/{projectId}/state/{stateId}
        [HttpPut("{stateId}")]
        public async Task<ActionResult> UpdateState([FromBody] StateDTO stateDTO, [FromRoute] string projectId, string stateId)
        {
            await _stateService.UpdateState(ObjectIdConverter.Validate(projectId), ObjectIdConverter.Validate(stateId), stateDTO);
            return NoContent();
        }

        // GET: api/company/{companyId}/project/{projectId}/state
        [HttpGet]
        public async Task<ActionResult<IList<StateDTO>>> GetStates()
        {
            throw new NotImplementedException();
        }

        // GET: api/company/{companyId}/project/{projectId}/state/{stateId}
        [HttpGet("{stateId}")]
        public async Task<ActionResult<StateDTO>> GetState(string stateId)
        {
            ObjectIdConverter.Validate(stateId);
            throw new NotImplementedException();
        }
    }
}
