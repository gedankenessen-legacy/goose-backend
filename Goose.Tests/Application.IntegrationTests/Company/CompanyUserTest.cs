using Goose.API;
using Goose.API.Repositories;
using Goose.Domain.DTOs;
using Goose.Domain.Models.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Goose.Tests.Application.IntegrationTests.Company
{
    [TestFixture]
    public class CompanyUserTest
    {
        private HttpClient _client;
        private WebApplicationFactory<Startup> _factory;
        private ICompanyRepository _companyRepository;
        private IUserRepository _userRepository;
        private SignInResponse signInObject;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new WebApplicationFactory<Startup>();
            _client = _factory.CreateClient();
            var scopeFactory = _factory.Server.Services.GetService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope())
            {
                _companyRepository = scope.ServiceProvider.GetService<ICompanyRepository>();
                _userRepository = scope.ServiceProvider.GetService<IUserRepository>();
            }
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await TestHelper.Instance.ClearCompany(_companyRepository, _userRepository);
        }

        [SetUp]
        public async Task Setup()
        {
            await TestHelper.Instance.ClearCompany(_companyRepository, _userRepository);
            signInObject = await TestHelper.Instance.GenerateCompany(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", signInObject.Token);
        }

        [Test]
        public async Task CreateUserTrue()
        {
            var company = (await _companyRepository.FilterByAsync(x => x.Name.Equals(TestHelper.FirmenName))).FirstOrDefault();
            var uri = $"api/companies/{company.Id}/users";

            PropertyUserLoginDTO user = GetUser("Phillip", "Schmidt", "Test123456", new List<RoleDTO>() { new RoleDTO() { Name = "Mitarbeiter" } });

            var response = await _client.PostAsync(uri, user.ToStringContent());
            var newUser = await response.Content.Parse<PropertyUserDTO>();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var uriUser = $"api/companies/{company.Id}/users/{newUser.User.Id}";

            response = await _client.GetAsync(uriUser);

            var exists = await response.Content.Parse<PropertyUserDTO>() != null;

            Assert.IsTrue(exists);
        }

        [Test]
        public async Task CreateUserWithOutCompanyRole()
        {
            var company = (await _companyRepository.FilterByAsync(x => x.Name.Equals(TestHelper.FirmenName))).FirstOrDefault();
            var uri = $"api/companies/{company.Id}/users";

            PropertyUserLoginDTO user = GetUser("Phillip", "Schmidt", "Test123456", new List<RoleDTO>() { new RoleDTO() { Name = "Mitarbeiter" } });

            var response = await _client.PostAsync(uri, user.ToStringContent());
            var newUser = await response.Content.Parse<PropertyUserDTO>();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var uriUser = $"api/companies/{company.Id}/users/{newUser.User.Id}";

            response = await _client.GetAsync(uriUser);

            var exists = await response.Content.Parse<PropertyUserDTO>() != null;

            Assert.IsTrue(exists);

            var signInNewUser = await SignIn(newUser.User.Username, "Test123456");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", signInNewUser.Token);

            response = await _client.PostAsync(uri, user.ToStringContent());

            Assert.IsFalse(response.IsSuccessStatusCode);
        }

        [Test]
        public async Task CreateUserFirstNameFalse()
        {
            var company = (await _companyRepository.FilterByAsync(x => x.Name.Equals(TestHelper.FirmenName))).FirstOrDefault();
            var uri = $"api/companies/{company.Id}/users";

            PropertyUserLoginDTO user = GetUser(" ", "Schmidt", "Test123456", new List<RoleDTO>() { new RoleDTO() { Name = "Mitarbeiter" } });

            var response = await _client.PostAsync(uri, user.ToStringContent());

            Assert.IsFalse(response.IsSuccessStatusCode);
        }

        [Test]
        public async Task CreateUserLastNameFalse()
        {
            var company = (await _companyRepository.FilterByAsync(x => x.Name.Equals(TestHelper.FirmenName))).FirstOrDefault();
            var uri = $"api/companies/{company.Id}/users";

            PropertyUserLoginDTO user = GetUser("Phillip", " ", "Test123456", new List<RoleDTO>() { new RoleDTO() { Name = "Mitarbeiter" } });

            var response = await _client.PostAsync(uri, user.ToStringContent());

            Assert.IsFalse(response.IsSuccessStatusCode);
        }

        private async Task<SignInResponse> SignIn(string userName, string password)
        {
            var uri = "/api/auth/signIn";
            SignInRequest signInRequest = new SignInRequest() { Username = userName, Password = password };
            var response = await _client.PostAsync(uri, signInRequest.ToStringContent());
            return await response.Content.Parse<SignInResponse>();
        }

        private PropertyUserLoginDTO GetUser(string firstname, string lastname, string password, List<RoleDTO> roles)
        {
            return new PropertyUserLoginDTO()
            {

                Firstname = firstname,
                Lastname = lastname,
                Password = password,
                Roles = roles
            };
        }
    }
}
