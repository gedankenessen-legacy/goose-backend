using Goose.API.Authorization.Requirements;
using Goose.API.Utils.Authentication;
using Goose.Domain.Models;
using Goose.Domain.Models.Projects;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Authorization.Handlers
{
    public class ProjectRoleHandler : RoleHandler<Project>
    {
    }
}
