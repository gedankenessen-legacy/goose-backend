using Goose.API;
using Goose.API.Repositories;
using Goose.Domain.DTOs;
using Goose.Domain.Models.Auth;
using Goose.Domain.Models.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Goose.Tests.Application.IntegrationTests.Issues
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

        #region T76

        [Test]
        public void CustomerCanWriteMessageTest()
        {
            throw new Exception("");
        }

        [Test]
        public void CustomerCanceledIssueTest()
        {
            throw new Exception("");
        }

        [Test]
        public void CustomerShouldNotBeAbleToStartTimeSheetTest()
        {
            throw new Exception("");
        }

        [Test]
        public void CustomerShouldNotBeAbleToEditTimeSheetTest()
        {
            throw new Exception("");
        }

        [Test]
        public void EmployeeCanWriteMessageTest()
        {
            throw new Exception("");
        }

        [Test]
        public void EmployeeCanEditStateOfIssueTest()
        {
            throw new Exception("");
        }

        [Test]
        public void EmployeeCanEditOwnTimeSheetsOfIssueTest()
        {
            throw new Exception("");
        }

        [Test]
        public void EmployeeCanAddSubIssueTest()
        {
            throw new Exception("");
        }

        [Test]
        public void ProjectLeaderCanCanceleIssueTest()
        {
            throw new Exception("");
        }

        [Test]
        public void ProjectLeaderCanEditOtherTimeSheetsOfIssueTest()
        {
            throw new Exception("");
        }

        [Test]
        public void CompanyOwnerCanCanceleIssueTest()
        {
            throw new Exception("");
        }

        [Test]
        public void CompanyOwnerCanEditOtherTimeSheetsOfIssueTest()
        {
            throw new Exception("");
        }

        #endregion

        #region T77
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
