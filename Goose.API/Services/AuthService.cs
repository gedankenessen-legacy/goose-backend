using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs;
using Goose.Domain.Models.Auth;
using Goose.Domain.Models.identity;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Goose.API.Utils;
using Goose.API.Repositories;

namespace Goose.API.Services
{
    public interface IAuthService
    {
        Task<SignInResponse> SignUpAsync(SignUpRequest signUpRequest);
        Task<SignInResponse> SignInAsync(SignInRequest signInRequest);
    }

    public class AuthService : IAuthService
    {
        private readonly ICompanyService _companyService;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IOptions<TokenSettings> _tokenSettings;

        public AuthService(ICompanyService companyService, IUserService userService, IUserRepository userRepository, IOptions<TokenSettings> tokenSettings)
        {
            _companyService = companyService;
            _userService = userService;
            _userRepository = userRepository;
            _tokenSettings = tokenSettings;
        }

        public async Task<SignInResponse> SignUpAsync(SignUpRequest signUpRequest)
        {
            // validate company name
            if (await _companyService.CompanyNameAvailableAsync(signUpRequest.CompanyName) is false)
                throw new HttpStatusException(StatusCodes.Status409Conflict, "A company with this name is already existing.");

            // create new user
            User newUser = new User()
            {
                Firstname = signUpRequest.Firstname,
                Lastname = signUpRequest.Lastname
            };

            // generate username
            newUser.Username = await GenerateUserNameAsync(newUser.Firstname, newUser.Lastname);

            // password hashed by BCrypt with workFactor of 11 => round about 150-300 ms depends on hardware.
            newUser.HashedPassword = BCrypt.Net.BCrypt.HashPassword(signUpRequest.Password);

            // save user
            await _userService.CreateNewUserAsync(newUser);

            // create company
            CompanyDTO newCompanyDTO = await _companyService.CreateCompanyAsync(signUpRequest.CompanyName, newUser.Id);

            // generate token
            var token = CreatToken(newUser);

            return new SignInResponse()
            {
                User = new UserDTO(newUser),
                Companies = await _companyService.GetCompaniesAsync(),
                Token = token
            };
        }

        public async Task<SignInResponse> SignInAsync(SignInRequest signInRequest)
        {
            // find user with username
            var user = await _userRepository.GetByUsernameAsync(signInRequest.Username);

            if (user is null)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "Cannot signIn."); // generic message in order to not let the clients know what is wrong.

            // verify password
            if (!BCrypt.Net.BCrypt.Verify(signInRequest.Password, user.HashedPassword))
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "Cannot signIn."); // generic message in order to not let the clients know what is wrong.

            // Generate Token
            var token = CreatToken(user);

            return new SignInResponse()
            {
                User = new UserDTO(user),
                Companies = await _companyService.GetCompaniesAsync(), // TODO: as soon the signin is enabled and the api is secured, the requests cann be restricted to the ressources that the client can view. Currently he receives all, even the ones he should not see.
                Token = token
            };
        }

        /// <summary>
        /// Creates a JWT-Token issued for the provided user.
        /// </summary>
        /// <param name="user">The Subject the token will be issued for</param>
        /// <returns>A JWT-Token with all user claims.</returns>
        private string CreatToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_tokenSettings.Value.Secret);

            var tokenHandler = new JwtSecurityTokenHandler();

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("userId", user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(_tokenSettings.Value.ExpireInHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generates a unique Username, based of the Firstname and the Lastname. If the Firstname letters are not enough to build a unique name, use numbers at the end of the Username.
        /// </summary>
        /// <param name="firstName">Firstname</param>
        /// <param name="lastName">Lastname</param>
        /// <returns>Unique Username with Firstname+Lastname plus number suffix if needed.</returns>
        private async Task<string> GenerateUserNameAsync(string firstName, string lastName)
        {
            int index = 0;
            string username;

            do
            {
                index++;

                if (index < firstName.Length)
                    username = firstName.Substring(0, index) + lastName;
                else
                    username = firstName + lastName + index.ToString();

                // Max 1000 users with same name and number suffix... enough for project 2...
                if (index > 1000) throw new HttpStatusException(StatusCodes.Status500InternalServerError, "Username cannot be generated.");
            }
            while (await _userService.UsernameAvailableAsync(username) is false);

            return username;
        }
    }
}
