using Goose.API.Services;
using Goose.Domain.DTOs;
using Goose.Domain.Models.Companies;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Goose.API.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompanyController : Controller
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        public async Task<ActionResult<IList<CompanyDTO>>> GetCompaniesAsync()
        {
            var companies = await _companyService.GetCompaniesAsync();
            return Ok(companies);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyDTO>> GetCompanyAsync(string id)
        {
            var company = await _companyService.GetCompanyAsync(id);
            return Ok(company);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CompanyDTO>> UpdateCompanyAsync(string id, [FromBody] CompanyDTO company)
        {
            var companyToUpdate = await _companyService.UpdateCompanyAsync(id, company);
            return Ok(companyToUpdate);
        }
    }
}
