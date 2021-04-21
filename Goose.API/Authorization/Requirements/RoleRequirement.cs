using Microsoft.AspNetCore.Authorization;
using Goose.Domain.Models.Identity;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
