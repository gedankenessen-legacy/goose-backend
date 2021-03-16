using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services;
using Goose.Domain.DTOs.issues;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Goose.API.Controllers
{
    [Route("api/projects/{projectId}/issues/")]
    [ApiController]
    public class IssuesController : Controller
    {
        //TODO user auth
        //TODO verify issue is in project
        private readonly IIssueService _issueService;

        public IssuesController(IIssueService issueService)
        {
            _issueService = issueService;
        }

        /// <summary>
        /// Returns every issue of a project
        /// </summary>
        /// <param name="id of project"></param>
        /// <returns>List of issues from a project or error 400</returns>
        [HttpGet]
        public async Task<ActionResult<IList<IssueDTO>>> GetAll([FromRoute] string projectId)
        {
            //var res = await _issueService.GetAllOfProject(new ObjectId(projectId));
            var res = await _issueService.GetAll();
            return Ok(res);
        }

        /// <summary>
        /// Returns specific issue
        /// </summary>
        /// <param name="projectId">id of project</param>
        /// <param name="id">id of issue</param>
        /// <returns>Returns issue or erro 400</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<IssueDTO>> Get([FromRoute] string projectId, [FromRoute] string id)
        {
            var projectObjId = projectId.TryParse();
            var objId = id.TryParse();

            var res = await _issueService.GetOfProject(projectObjId, objId);
            return res == null ? NotFound() : Ok(res);
        }

        /// <summary>
        /// Creates an issue in db and returns created issue
        /// </summary>
        /// <param name="issue">Issue from body</param>
        /// <param name="projectId">project id</param>
        /// <returns>new issue or error 400</returns>
        [HttpPost]
        public async Task<ActionResult<IssueDTO>> Post([FromBody] IssueDTO issue, [FromRoute] string projectId)
        {
            var res = await _issueService.Create(issue);
            return Created($"api/projects/{projectId}/issues/{res.Id}", res);
        }

        /// <summary>
        /// Creates or updates an issue
        /// </summary>
        /// <param name="issue">Issue</param>
        /// <param name="projectId">project id</param>
        /// <param name="id">id of issue. If an issue is created, it will not have the id specified here.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<IssueDTO>> Put([FromBody] IssueDTO issue, [FromRoute] string projectId,
            [FromRoute] string id)
        {
            await _issueService.CreateOrUpdate(issue);
            return NoContent();
        }


        /// <summary>
        /// Deletes an issue of a project
        /// </summary>
        /// <param name="projectId">project id</param>
        /// <param name="id">issue id</param>
        /// <returns>returns http code 204 on success or 400 on error</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<IssueDTO>> Delete([FromRoute] string projectId, [FromRoute] string id)
        {
            var res = await _issueService.Delete(id.TryParse());
            if (res.DeletedCount > 0)
                return NoContent();
            return BadRequest();
        }
    }
}

namespace System
{
    static class Extentions
    {
        public static ObjectId TryParse(this string id)
        {
            if (ObjectId.TryParse(id, out ObjectId newId) is false)
                throw new Exception("Cannot parse issue string id to a valid object id.");
            //  new HttpStatusException(StatusCodes.Status400BadRequest, "Cannot parse issue string id to a valid object id.");
            return newId;
        }
    }
}