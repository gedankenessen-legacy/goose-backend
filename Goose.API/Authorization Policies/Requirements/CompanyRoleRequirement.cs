using Goose.Domain.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Authorization_Policies.Requirements
{
    /// <summary>
    /// To achive the requirment, the user needs to have the defined role (<see cref="RoleName"/> and <see cref="RoleId"/>)
    /// </summary>
    public class CompanyRoleRequirement : IAuthorizationRequirement
    {
        public string RoleName { get; set; }
        public ObjectId? RoleId { get; set; } //? With static role ids we could save multiple database calls (also easier code). On a real world we could once the app is started cache the roles in memory and modify on CRUD but for p2 it is overkill.

        public CompanyRoleRequirement(string roleName)
        {
            RoleName = roleName;
        }

        public CompanyRoleRequirement(string roleName, ObjectId roleId) : this(roleName)
        {
            RoleId = roleId;
        }
    }

    /// <summary>
    /// This class is a shortcut for certain company roles. The properties returnes a concrete <see cref="CompanyRoleRequirement"/>
    /// </summary>
    public static class CompanyRolesRequirement
    {
        public readonly static CompanyRoleRequirement CompanyOwner = new(Role.CompanyRole, new ObjectId("604a3420db17824bca29698f"));
        public readonly static CompanyRoleRequirement CompanyCustomer = new(Role.CustomerRole, new ObjectId("605cc95dd37ccd8527c2ead7"));
    }
}
