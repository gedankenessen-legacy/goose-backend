using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services;
using Goose.Domain.DTOs.issues;
using Goose.Domain.Models.tickets;
using Microsoft.AspNetCore.Mvc;

namespace Goose.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class IssuesController : Controller
    {
        private readonly IIssueService _issueService;

        public IssuesController(IIssueService issueService)
        {
            _issueService = issueService;
        }


        [HttpGet]
        public async Task<ActionResult<IList<IssueDTO>>> GetAll()
        {
            var res = await _issueService.GetAllIssues();
            return ActionResultOk(res);
        }


        private ActionResult<E> ActionResultOk<E>(E e)
        {
            return Ok(e);
        }
    }
}