using Goose.API.Authorization.Requirements;
using Goose.Domain.Models.Projects;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Authorization.Handlers
{
    public class ProjectRoleHandler : AuthorizationHandler<ProjectRoleRequirement, Project>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ProjectRoleRequirement requirement, Project resource)
        {
            throw new NotImplementedException();
        }
    }
}
