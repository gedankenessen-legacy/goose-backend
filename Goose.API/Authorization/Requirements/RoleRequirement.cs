using Microsoft.AspNetCore.Authorization;
using Goose.Domain.Models.Identity;

namespace Goose.API.Authorization.Requirements
{
    /// <summary>
    /// To achive the requirment, the user needs to have the defined role.
    /// </summary>
    public class RoleRequirement : IAuthorizationRequirement
    {
        public Role Role { get; set; }

        public RoleRequirement(Role role)
        {
            Role = role;
        }
    }
}
