using Goose.API.Services;
using Goose.Domain.Models.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Goose.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Use this Endpoint to register a user with a company. This endpoint creates a User and assignes him to the company which will gets created by name provided.
        /// </summary>
        /// <param name="signUpRequest">The request needs to have all properties and keeps all validation rules (e.g.: Password: 8 chars and needs at least one number)</param>
        [HttpPost("signUp")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest signUpRequest)
        {
            var result = await _authService.SignUpAsync(signUpRequest);
            return Ok(result);
        }

        /// <summary>
        /// Use this Endpoint to sign in with a user. This endpoint returns a valid JWT-Token on successfully sign in.
        /// </summary>
        [HttpPost("signIn")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest signInRequest)
        {
            var result = await _authService.SignInAsync(signInRequest);
            return Ok(result);
        }
    }
}
