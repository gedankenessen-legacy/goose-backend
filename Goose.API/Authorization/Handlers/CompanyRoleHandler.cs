using Goose.API.Authorization.Requirements;
using Goose.API.Services;
using Goose.API.Utils.Authentication;
using Goose.Domain.Models.Companies;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace Goose.API.Authorization.Handlers
{
    /// <summary>
    /// This handler is used to validate if the user matches the requested requirement (<see cref="CompanyRoleRequirement"/>).
    /// </summary>
    public class CompanyRoleHandler : AuthorizationHandler<CompanyRoleRequirement, Company>
    {
        private readonly ICompanyService _companyService;

        public CompanyRoleHandler(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CompanyRoleRequirement requirement, Company resource)
        {
            ObjectId userId = context.User.GetUserId();

            if (await _companyService.UserHasRoleInCompany(userId, resource.Id, requirement.RoleName))
            {
                context.Succeed(requirement);
            }
        }
    }
}
