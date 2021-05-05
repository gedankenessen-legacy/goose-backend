using Goose.Domain.Models.Identity;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Authorization.Requirements
{
    public class ProjectRoleRequirement : RoleRequirement
    {
        public ProjectRoleRequirement(Role projectRole) : base(projectRole) { }
    }

    /// <summary>
    /// This class is a shortcut for certain company roles. The properties returnes a concrete <see cref="CompanyRoleRequirement"/>
    /// </summary>
    public static class ProjectRolesRequirement
    {
        public readonly static ProjectRoleRequirement LeaderRequirement = new(Role.ProjectLeaderRole);
        public readonly static ProjectRoleRequirement CustomerRequirement = new(Role.CustomerRole);
        public readonly static ProjectRoleRequirement EmployeeRequirement = new(Role.EmployeeRole);
        public readonly static ProjectRoleRequirement ReadonlyEmployeeRequirement = new(Role.ReadonlyEmployeeRole);
    }
}
