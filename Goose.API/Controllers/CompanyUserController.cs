using Goose.API.Services;
using Goose.Domain.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Controllers
{
    [Route("api/companies/{companyId}/users")]
    [ApiController]
    public class CompanyUserController : Controller
    {
        private readonly ICompanyUserService _companyUserService;

        public CompanyUserController(ICompanyUserService companyUserService)
        {
            _companyUserService = companyUserService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IList<PropertyUserDTO>>> GetCompanyUsers([FromRoute] string companyId)
        {
            var users = await _companyUserService.GetCompanyUsersAsync(companyId);
            return Ok(users);
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PropertyUserDTO>> GetCompanyUsers([FromRoute] string companyId, string userId)
        {
            var user = await _companyUserService.GetCompanyUserAsync(companyId, userId);
            return Ok(user);
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PropertyUserDTO>> CreateCompanyUser([FromRoute] string companyId, PropertyUserLoginDTO user)
        {
            var newUser = await _companyUserService.CreateComapanyUserAsync(companyId, user);
            return Ok(newUser);
        }


        [HttpPut("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PropertyUserDTO>> UpdateCompanyUser([FromRoute] string companyId, string userId, PropertyUserLoginDTO user)
        {
            var updatedUser = await _companyUserService.UpdateComapanyUserAsync(companyId, userId, user);
            return Ok(updatedUser);
        }
    }
}
