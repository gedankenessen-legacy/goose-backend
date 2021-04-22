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
        public ProjectRoleRequirement(string roleName) : base(roleName) { }
        public ProjectRoleRequirement(string roleName, ObjectId roleId) : base(roleName, roleId) { }
    }

    /// <summary>
    /// This class is a shortcut for certain company roles. The properties returnes a concrete <see cref="CompanyRoleRequirement"/>
    /// </summary>
    public static class ProjectRolesRequirement
    {
        public readonly static ProjectRoleRequirement LeaderRequirement = new(Role.ProjectLeaderRole, new ObjectId("60709abc53608b0ba47360ff"));
        public readonly static ProjectRoleRequirement CustomerRequirement = new(Role.CustomerRole, new ObjectId("605cc95dd37ccd8527c2ead7"));
        public readonly static ProjectRoleRequirement EmployeeRequirement = new(Role.EmployeeRole, new ObjectId("605cc555e11e3fa9088d4dd4"));
        public readonly static ProjectRoleRequirement ReadonlyEmployeeRequirement = new(Role.ReadonlyEmployeeRole, new ObjectId("607aedecbba3b233b8582ae7"));
    }
}
