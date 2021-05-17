using Goose.API.Authorization.Requirements;
using Goose.API.Utils.Authentication;
using Goose.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Authorization.Handlers
{
    public abstract class RoleHandler<T> : AuthorizationHandler<RoleRequirement, T> where T : IPropertyUsers 
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement, T resource)
        {
            ObjectId userId = context.User.GetUserId();
            ObjectId requiredRoleId = requirement.Role.Id;
            IList<PropertyUser> propertyUsers = resource.Users;

            PropertyUser reqestedUser = propertyUsers?.FirstOrDefault(pu => pu.UserId.Equals(userId));

            // if user complies the required role.
            if (reqestedUser is not null && reqestedUser.RoleIds.Any(ri => ri.Equals(requiredRoleId)))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
