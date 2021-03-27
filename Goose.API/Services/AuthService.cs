using Goose.Domain.Models.Auth;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Services
{
    public interface IAuthService
    {
        public Task<SignInResponse> SignUpAsync(SignUpRequest signUpRequest);
    }

    public class AuthService : IAuthService
    {
        public Task<SignInResponse> SignUpAsync(SignUpRequest signUpRequest)
        {
            // validate input
            //if (string.IsNullOrEmpty(signUpRequest.Firstname))
            //    throw HttpStatusException(StatusCodes.Status400BadRequest, "");
            //if (string.IsNullOrEmpty(signUpRequest.Lastname))

            //if (string.IsNullOrEmpty(signUpRequest.CompanyName))


            // validate company name

            // generate username

            // hash password

            // generate token

            return null;
        }
    }
}
