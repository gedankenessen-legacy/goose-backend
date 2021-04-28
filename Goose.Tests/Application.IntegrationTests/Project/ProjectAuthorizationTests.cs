using Goose.API;
using Goose.API.Repositories;
using Goose.API.Utils;
using Goose.Domain.DTOs;
using Goose.Domain.Models.Auth;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Projects;
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
    public class IssueAuthorizationTests
    {
        private HttpClient _client;
        private WebApplicationFactory<Startup> _factory;

        private ICompanyRepository _companyRepository;
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

        #endregion

        #region T82

        [Test, Order(1)]
        public async Task EmployeeCreatesOwnState()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyEmployeeSignIn.Token);

            var response = await CreateStateInProject("EmployeeCreatedState");

            employeeState = await response.Content.Parse<StateDTO>();

            Assert.IsTrue(response.IsSuccessStatusCode, "Employee was not able to create a custom state in his project!");
        }

        [Test, Order(2)]
        public async Task EmployeeEditsOwnState()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyEmployeeSignIn.Token);

            employeeState.Name = "edited";

            var response = await EditStateInProject(employeeState);

            response.EnsureSuccessStatusCode();

            Assert.True(response.IsSuccessStatusCode, $"{response.StatusCode}: Employee was not able to edit a custom state in his project!");
        }

        [Test, Order(3)]
        public async Task EmployeeRemovesOwnState()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyEmployeeSignIn.Token);

            var response = await RemoveStateInProject(employeeState);

            response.EnsureSuccessStatusCode();

            Assert.True(response.IsSuccessStatusCode, $"{response.StatusCode}: Employee was not able to remove a custom state in his project!");
        }

        [Test, Order(1)]
        public async Task CustomerIsNotAllowedToCreatesOwnState()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyClientSignIn.Token);

            var response = await CreateStateInProject("ClientCreatedState");

            var error = await response.Content.Parse<ErrorResponse>();

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, "Customer was able to create a custom state for a project!\n" + error.Message);
        }

        [Test, Order(2)]
        public async Task CustomerIsNotAllowedToEditsOwnState()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyClientSignIn.Token);

            employeeState.Name = "edited";

            var response = await EditStateInProject(employeeState);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, "Customer was able to edit a custom state for a project!");
        }

        [Test, Order(3)]
        public async Task CustomerIsNotAllowedToRemovesOwnState()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyClientSignIn.Token);

            var response = await RemoveStateInProject(employeeState);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, "Customer was able to remove a custom state for a project!");
        }

        #endregion
        
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

        private async Task<HttpResponseMessage> CreateStateInProject(string stateName = "TestState", StateDTO newState = null)
        {
            newState ??= new StateDTO()
            {
                Name = stateName,
                Phase = State.NegotiationPhase
            };

            return await _client.PostAsync($"api/projects/{createdProject.Id}/states", newState.ToStringContent());
        }

        private async Task<HttpResponseMessage> EditStateInProject(StateDTO stateEdited)
        {
            return await _client.PutAsync($"api/projects/{createdProject.Id}/states/{stateEdited.Id}", stateEdited.ToStringContent());
        }

        private async Task<HttpResponseMessage> RemoveStateInProject(StateDTO stateToDelete)
        {
            return await _client.DeleteAsync($"api/projects/{createdProject.Id}/states/{stateToDelete.Id}");
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
