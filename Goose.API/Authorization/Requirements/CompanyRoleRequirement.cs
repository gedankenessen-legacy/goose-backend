using Goose.Domain.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Authorization.Requirements
{
    public class CompanyRoleRequirement : RoleRequirement
    {
        public CompanyRoleRequirement(string roleName) : base (roleName) {}
        public CompanyRoleRequirement(string roleName, ObjectId roleId) : base (roleName, roleId) { }
    }

    /// <summary>
    /// This class is a shortcut for certain company roles. The properties returnes a concrete <see cref="CompanyRoleRequirement"/>
    /// </summary>
    public static class CompanyRolesRequirement
    {
        public readonly static CompanyRoleRequirement CompanyOwner = new(Role.CompanyRole.Name, Role.CompanyRole.Id);
        public readonly static CompanyRoleRequirement CompanyCustomer = new(Role.CustomerRole.Name, Role.CustomerRole.Id);
    }
}
