using Goose.API;
using Goose.API.Repositories;
using Goose.Domain.DTOs;
using Goose.Domain.Models.Auth;
using Goose.Domain.Models.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Goose.Tests.Application.IntegrationTests.Project
{
    [TestFixture]
    public class ProjectAuthorizationTests
    {
        private HttpClient _client;
        private WebApplicationFactory<Startup> _factory;

        private ICompanyRepository _companyRepository;
        private IUserRepository _userRepository;
        private IProjectRepository _projectRepository;

        private SignInResponse companyOwnerSignIn;
        private SignInResponse companyClientSignIn;
        private SignInResponse companyEmployeeSignIn;
        private ProjectDTO createdProject;
        private StateDTO employeeState;

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
                _projectRepository = scope.ServiceProvider.GetService<IProjectRepository>();
            }
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await Clear();
        }

        [SetUp]
        public async Task Setup()
        {
            await Clear();
            await Generate();
        }

        #region T10

        [Test]
        public async Task AssignCustomerToProjektAsCustomerTest()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyClientSignIn.Token);
            var response = await AssignUserToProjectAsync($"api/projects/{createdProject.Id}/users/{companyClientSignIn.User.Id}");
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, "Customer was able to create a customer for a project!");
        }

        [Test]
        public async Task AssignCustomerToProjektAsOwnerTest()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyOwnerSignIn.Token);

            var response = await AssignUserToProjectAsync($"api/projects/{createdProject.Id}/users/{companyClientSignIn.User.Id}");

            Assert.IsTrue(response.IsSuccessStatusCode, "Company owner was not be able to create a customer for a project!");
        }

        [Test]
        public async Task EmployeeCreatesOwnState()
        {
            Assert.IsTrue(false);
        }

        [Test]
        public async Task EmployeeEditsOwnState()
        {
            Assert.IsTrue(false);
        }

        [Test]
        public async Task EmployeeRemovesOwnState()
        {
            Assert.IsTrue(false);
        }

        private async Task<HttpResponseMessage> QueryState

        private async Task<HttpResponseMessage> AssignUserToProjectAsync(string uri, PropertyUserDTO? propertyUser = null)
        {
            propertyUser ??= new PropertyUserDTO()
            {
                User = companyClientSignIn.User,
                Roles = new List<RoleDTO>() {
                        new RoleDTO (Role.CustomerRole)
                }
            };

            return await _client.PutAsync(uri, propertyUser.ToStringContent());
        }

        private async Task Clear()
        {
            await TestHelper.Instance.ClearCompany();
            await TestHelper.Instance.ClearProject();
        }

        private async Task Generate()
        {
            companyOwnerSignIn = await TestHelper.Instance.GenerateCompany(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyOwnerSignIn.Token);
            createdProject = await TestHelper.Instance.GenerateProject(_client);

            PropertyUserLoginDTO customerSignIn = new()
            {
                Firstname = "Test",
                Lastname = "Kunde",
                Password = "string1234",
                Roles = new List<RoleDTO>() {
                        new RoleDTO (Role.CustomerRole)
                }

            };

            PropertyUserLoginDTO employeeSignIn = new()
            {
                Firstname = "Test",
                Lastname = "Mitarbeiter",
                Password = "string1234",
                Roles = new List<RoleDTO>() {
                        new RoleDTO (Role.EmployeeRole)
                }
            };

            companyClientSignIn = await TestHelper.Instance.GenerateUserForCompany(_client, _companyRepository, customerSignIn);
            companyClientSignIn = await TestHelper.Instance.SignIn(_client, new() { Username = companyClientSignIn.User.Username, Password = customerSignIn.Password });

            companyEmployeeSignIn = await TestHelper.Instance.GenerateUserForCompany(_client, _companyRepository, employeeSignIn);
            companyEmployeeSignIn = await TestHelper.Instance.SignIn(_client, new() { Username = companyEmployeeSignIn.User.Username, Password = employeeSignIn.Password });

            await AssignUserToProjectAsync($"api/projects/{createdProject.Id}/users/{companyClientSignIn.User.Id}", new()
            {
                User = new() { Id = companyClientSignIn.User.Id },
                Roles = new List<RoleDTO>() {
                        new RoleDTO (Role.CustomerRole)
                }
            });

            await AssignUserToProjectAsync($"api/projects/{createdProject.Id}/users/{companyEmployeeSignIn.User.Id}", new()
            {
                User = new() { Id = companyEmployeeSignIn.User.Id },
                Roles = new List<RoleDTO>() {
                        new RoleDTO (Role.EmployeeRole)
                }
            });
        }
    }
}
