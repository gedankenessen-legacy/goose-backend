using Goose.API.Authorization.Requirements;
using Goose.API.Services;
using Goose.API.Utils.Authentication;
using Goose.Domain.Models;
using Goose.Domain.Models.Companies;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Authorization.Handlers
{
    /// <summary>
    /// This handler is used to validate if the user matches the requested requirement (<see cref="CompanyRoleRequirement"/>).
    /// </summary>
    public class CompanyRoleHandler : RoleHandler<Company>
    {
    }
}
