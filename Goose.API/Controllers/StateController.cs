using Goose.API.Services;
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
        public async Task<ActionResult<StateDTO>> CreateState([FromBody] StateDTO stateDTO)
        {
            throw new NotImplementedException();
        }

        // PUT: api/company/{companyId}/project/{projectId}/state/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateState(string id, [FromBody] StateDTO stateDTO)
        {
            throw new NotImplementedException();
        }

        // GET: api/company/{companyId}/project/{projectId}/state
        [HttpGet]
        public async Task<ActionResult<IList<StateDTO>>> GetStates()
        {
            throw new NotImplementedException();
        }

        // GET: api/company/{companyId}/project/{projectId}/state/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<StateDTO>> GetState(string id)
        {
            throw new NotImplementedException();
        }
    }
}
