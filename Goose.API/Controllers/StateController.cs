using Goose.API.Services;
using Goose.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Controllers
{
    [Route("api/companies/{companyId}/projects/{projectId}/states")]
    public class StateController : ControllerBase
    {
        private readonly IStateService _stateService;

        public StateController(IStateService stateService)
        {
            _stateService = stateService;
        }

        // POST: api/companies/{companyId}/projects/{projectId}/states/
        [HttpPost]
        public async Task<ActionResult<StateDTO>> CreateState([FromBody] StateDTO stateDTO)
        {
            throw new NotImplementedException();
        }

        // PUT: api/companies/{companyId}/projects/{projectId}/states/{stateId}
        [HttpPut("{stateId}")]
        public async Task<ActionResult> UpdateState(string stateId, [FromBody] StateDTO stateDTO)
        {
            throw new NotImplementedException();
        }

        // GET: api/companies/{companyId}/projects/{projectId}/states
        [HttpGet]
        public async Task<ActionResult<IList<StateDTO>>> GetStates()
        {
            throw new NotImplementedException();
        }

        // GET: api/companies/{companyId}/projects/{projectId}/states/{stateId}
        [HttpGet("{stateId}")]
        public async Task<ActionResult<StateDTO>> GetState(string stateId)
        {
            throw new NotImplementedException();
        }
    }
}
