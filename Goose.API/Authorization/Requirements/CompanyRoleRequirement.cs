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
        public CompanyRoleRequirement(Role companyRole) : base(companyRole) { }
    }

    /// <summary>
    /// This class is a shortcut for certain company roles. The properties returnes a concrete <see cref="CompanyRoleRequirement"/>
    /// </summary>
    public static class CompanyRolesRequirement
    {
        public readonly static CompanyRoleRequirement CompanyOwner = new(Role.CompanyRole);
        public readonly static CompanyRoleRequirement CompanyCustomer = new(Role.CustomerRole);
        public readonly static CompanyRoleRequirement CompanyEmployee = new(Role.EmployeeRole);
    }
}
