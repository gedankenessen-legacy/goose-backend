using Goose.API.Utils;
using Goose.API.Utils.Exceptions;
using Goose.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;

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

        public static void ThrowErrorIfAllFailed(this AuthorizationResult authorizationResult, Dictionary<IAuthorizationRequirement, string> errorMap)
        {
            if (authorizationResult.Succeeded)
            {
                // All requirements succeded, no need to check every single one
                return;
            }

            if (authorizationResult.Failure.FailedRequirements.Count() < errorMap.Count)
            {
                // Not all Requirements have failed
                return;
            }

            var errorMsg = "";

            foreach (var failedRequirement in authorizationResult.Failure.FailedRequirements)
            {
                if (errorMap.TryGetValue(failedRequirement, out string errorMessage))
                {
                    errorMsg += errorMessage;
                }
                else
                {
                    // Fallback msg
                    errorMsg = "You are missing a requirements, in order to process this request.";
                }

                errorMsg += '\n';
            }

            throw new HttpStatusException(StatusCodes.Status403Forbidden, errorMsg);
        }

        public static IList<ObjectId> RolesOfUser(params PropertyUser[] propertyUsers)
        {
            IList<ObjectId> roles = new List<ObjectId>();

            if (propertyUsers is null) return roles;

            foreach (var propertyUser in propertyUsers)
            {
                if (propertyUser is not null)
                    roles = roles.ConcatOrSkip(propertyUser.RoleIds);
            }

            return roles;
        }
    
    }
}
