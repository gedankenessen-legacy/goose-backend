using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Goose.API;
using Goose.Domain.DTOs;
using Goose.Domain.Models.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.Company
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class CompanyUserTest
    {
        private sealed class TestScope : IDisposable
        {
            public HttpClient client;
            public WebApplicationFactory<Startup> _factory;
            public SignInResponse signInObject;
            public CompanyDTO company => signInObject.Companies[0];
            public NewTestHelper helper;

            public TestScope()
            {
                Task.Run(() =>
                {
                    _factory = new WebApplicationFactory<Startup>();
                    client = _factory.CreateClient();

                    helper = new NewTestHelper(client);
                    signInObject = helper.GenerateCompany().Parse<SignInResponse>().Result;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", signInObject.Token);
                }).Wait();
            }

            public void Dispose()
            {
                client?.Dispose();
                _factory?.Dispose();
                helper.Dispose();
            }
        }

        [Test]
        public async Task CreateUserTrue()
        {
            using (var scope = new TestScope())
            {
                var uri = $"api/companies/{scope.company.Id}/users";

                PropertyUserLoginDTO user = GetUser("Phillip", "Schmidt", "Test123456", new List<RoleDTO>() {new RoleDTO() {Name = "Mitarbeiter"}});

                var response = await scope.client.PostAsync(uri, user.ToStringContent());
                var newUser = await response.Content.Parse<PropertyUserDTO>();

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                var uriUser = $"api/companies/{scope.company.Id}/users/{newUser.User.Id}";

                response = await scope.client.GetAsync(uriUser);

                var exists = await response.Content.Parse<PropertyUserDTO>() != null;

                Assert.IsTrue(exists);
            }
        }

        [Test]
        public async Task CreateUserWithOutCompanyRole()
        {
            using (var scope = new TestScope())
            {
                var uri = $"api/companies/{scope.company.Id}/users";

                PropertyUserLoginDTO user = GetUser("Phillip", "Schmidt", "Test123456", new List<RoleDTO>() {new RoleDTO() {Name = "Mitarbeiter"}});

                var response = await scope.client.PostAsync(uri, user.ToStringContent());
                var newUser = await response.Content.Parse<PropertyUserDTO>();

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                var uriUser = $"api/companies/{scope.company.Id}/users/{newUser.User.Id}";

                response = await scope.client.GetAsync(uriUser);

                var exists = await response.Content.Parse<PropertyUserDTO>() != null;

                Assert.IsTrue(exists);

                var signInNewUser = await SignIn(scope.client, newUser.User.Username, "Test123456");

                scope.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", signInNewUser.Token);

                response = await scope.client.PostAsync(uri, user.ToStringContent());

                Assert.IsFalse(response.IsSuccessStatusCode);
            }
        }

        [Test]
        public async Task CreateUserFirstNameFalse()
        {
            using (var scope = new TestScope())
            {
                var uri = $"api/companies/{scope.company.Id}/users";

                PropertyUserLoginDTO user = GetUser(" ", "Schmidt", "Test123456", new List<RoleDTO>() {new RoleDTO() {Name = "Mitarbeiter"}});

                var response = await scope.client.PostAsync(uri, user.ToStringContent());

                Assert.IsFalse(response.IsSuccessStatusCode);
            }
        }

        [Test]
        public async Task CreateUserLastNameFalse()
        {
            using (var scope = new TestScope())
            {
                var uri = $"api/companies/{scope.company.Id}/users";

                PropertyUserLoginDTO user = GetUser("Phillip", " ", "Test123456", new List<RoleDTO>() {new RoleDTO() {Name = "Mitarbeiter"}});

                var response = await scope.client.PostAsync(uri, user.ToStringContent());

                Assert.IsFalse(response.IsSuccessStatusCode);
            }
        }

        private async Task<SignInResponse> SignIn(HttpClient client, string userName, string password)
        {
            var uri = "/api/auth/signIn";
            SignInRequest signInRequest = new SignInRequest() {Username = userName, Password = password};
            var response = await client.PostAsync(uri, signInRequest.ToStringContent());
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