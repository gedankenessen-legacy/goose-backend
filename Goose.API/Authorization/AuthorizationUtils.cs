using Goose.API.Authorization.Requirements;
using Goose.API.Utils.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Authorization
{
    public static class AuthorizationUtils
    {
        public static void ThrowErrorForFailedRequirements(this AuthorizationResult authorizationResult, Dictionary<IAuthorizationRequirement, string> errorMap)
        {
            // if result is successfull or do not serve a failiture, we do not have to throws errors.
            if (authorizationResult.Succeeded || authorizationResult.Failure is null)
                return;

            // for each failied req. throw an error if served, else throw generic error. 
            foreach (var failedRequirement in authorizationResult.Failure.FailedRequirements)
            {
                if (errorMap.TryGetValue(failedRequirement, out string errorMessage))          
                    throw new HttpStatusException(StatusCodes.Status403Forbidden, errorMessage);
          
                // fallback error message.
                throw new HttpStatusException(StatusCodes.Status403Forbidden, "You are missing one or more requirements, in order to process this request.");    
            }
        }
    }
}
