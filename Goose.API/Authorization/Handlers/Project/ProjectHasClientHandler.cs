using Goose.Domain.Models.Companies;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Authorization.Handlers.Project
{
    public class ProjectHasClientHandler : AuthorizationHandler<ProjectHasClientRequirement, Company>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ProjectHasClientRequirement requirement, Company resource)
        {
            //if (resource != null)
            //    context.Succeed(requirement);

            return Task.CompletedTask;
        }


    }

    // Simple requirments (without properties) can be placed inside of the handler.
    public class ProjectHasClientRequirement : IAuthorizationRequirement { }
}
