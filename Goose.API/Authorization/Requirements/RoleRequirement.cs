using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Authorization.Requirements
{
    /// <summary>
    /// To achive the requirment, the user needs to have the defined role (<see cref="RoleName"/> and <see cref="RoleId"/>)
    /// </summary>
    public class RoleRequirement : IAuthorizationRequirement
    {
        public string RoleName { get; set; }
        public ObjectId? RoleId { get; set; } //? With static role ids we could save multiple database calls (also easier code). On a real world we could once the app is started cache the roles in memory and modify on CRUD but for p2 it is overkill.

        public RoleRequirement(string roleName)
        {
            RoleName = roleName;
        }

        public RoleRequirement(string roleName, ObjectId roleId) : this(roleName)
        {
            RoleId = roleId;
        }
    }
}
