using Goose.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Goose.API.Services
{
    abstract public class AuthorizableService
    {
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly IAuthorizationService _authorizationService;

        public AuthorizableService(IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        protected async Task AuthenticateRequirmentAsync<T>(T ressource, OperationAuthorizationRequirement requirment, string errorMessage = "You are missing rights to proceed this call.")
        {
            Dictionary<IAuthorizationRequirement, string> requirementsWithErrors = new()
            {
                { requirment, errorMessage }
            };

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, ressource, requirementsWithErrors.Keys);
            authorizationResult.ThrowErrorForFailedRequirements(requirementsWithErrors);
        }
    }
}
