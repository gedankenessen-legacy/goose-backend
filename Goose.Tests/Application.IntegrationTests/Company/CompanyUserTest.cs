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
       
        private class SimpleTestHelperBuilderCompanyUser : SimpleTestHelperBuilderBase
        {
            public override async Task<SimpleTestHelper> Build()
            {
                var simpleHelper = new SimpleTestHelper(_client);
                await simpleHelper.SignUp(_signUpRequest);
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", simpleHelper.SignIn.Token);
                return simpleHelper;
            }
        }

        [Test]
        public async Task CreateUserTrue()
        {
            using (var scope = new TestScope(new SimpleTestHelperBuilderCompanyUser()))
            {
                var uri = $"api/companies/{scope.Helper.Company.Id}/users";

                PropertyUserLoginDTO user = GetUser("Phillip", "Schmidt", "Test123456", new List<RoleDTO>() {new RoleDTO() {Name = "Mitarbeiter"}});

                var response = await scope.client.PostAsync(uri, user.ToStringContent());
                var newUser = await response.Content.Parse<PropertyUserDTO>();

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                var uriUser = $"api/companies/{scope.Helper.Company.Id}/users/{newUser.User.Id}";

                response = await scope.client.GetAsync(uriUser);

                var exists = await response.Content.Parse<PropertyUserDTO>() != null;

                Assert.IsTrue(exists);
            }
        }

        [Test]
        public async Task CreateUserWithOutCompanyRole()
        {
            using (var scope = new TestScope(new SimpleTestHelperBuilderCompanyUser()))
            {
                var uri = $"api/companies/{scope.Helper.Company.Id}/users";

                PropertyUserLoginDTO user = GetUser("Phillip", "Schmidt", "Test123456", new List<RoleDTO>() {new RoleDTO() {Name = "Mitarbeiter"}});

                var response = await scope.client.PostAsync(uri, user.ToStringContent());
                var newUser = await response.Content.Parse<PropertyUserDTO>();

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                var uriUser = $"api/companies/{scope.Helper.Company.Id}/users/{newUser.User.Id}";

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
            using (var scope = new TestScope(new SimpleTestHelperBuilderCompanyUser()))
            {
                var uri = $"api/companies/{scope.Helper.Company.Id}/users";

                PropertyUserLoginDTO user = GetUser(" ", "Schmidt", "Test123456", new List<RoleDTO>() {new RoleDTO() {Name = "Mitarbeiter"}});

                var response = await scope.client.PostAsync(uri, user.ToStringContent());

                Assert.IsFalse(response.IsSuccessStatusCode);
            }
        }

        [Test]
        public async Task CreateUserLastNameFalse()
        {
            using (var scope = new TestScope(new SimpleTestHelperBuilderCompanyUser()))
            {
                var uri = $"api/companies/{scope.Helper.Company.Id}/users";

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