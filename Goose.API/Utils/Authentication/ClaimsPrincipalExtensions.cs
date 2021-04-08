using Goose.API.Utils.Exceptions;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using Goose.API.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Goose.API.Utils.Authentication
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Retrieve the id of the current signed in user. 
        /// e.g.: <i>userId = ...HttpContext.User.GetUserId();</i>
        /// </summary>
        public static ObjectId GetUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValueObjectId(UserClaimTypes.UserIdClaimType);
        }

        public static string FindFirstValue(this ClaimsPrincipal principal, string claimType, bool throwIfNotFound = false)
        {
            if (principal is null || principal.Claims.Any() is false)
                throw new HttpStatusException(StatusCodes.Status401Unauthorized, "Please SignIn first.");

            var value = principal.FindFirst(claimType)?.Value;

            if (throwIfNotFound && string.IsNullOrWhiteSpace(value))
            {
                throw new HttpStatusException(StatusCodes.Status403Forbidden, "Request is missing claim");
            }

            return value;
        }

        public static ObjectId FindFirstValueObjectId(this ClaimsPrincipal principal, string claimType, bool throwIfNotFound = false)
        {
            return Validators.Validators.ValidateObjectId(FindFirstValue(principal, claimType, throwIfNotFound), "UserId could be parsed.");
        }
    }
}
