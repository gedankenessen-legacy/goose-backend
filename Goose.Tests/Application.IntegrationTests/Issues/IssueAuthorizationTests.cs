using Goose.API;
using Goose.API.Repositories;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Auth;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Issues;
using Goose.Domain.Models.Projects;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
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
        private IssueDTO issue;
        private IssueDTO internalIssue;

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
            await TestHelper.Instance.ClearAll();
        }

        [SetUp]
        public async Task Setup()
        {
            await TestHelper.Instance.ClearAll();
            await Generate();
        }

        #region T76

        [Test, Order(1)]
        public async Task CustomerCanWriteMessageTestAsync()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyClientSignIn.Token);

            var uri = $"/api/issues/{issue.Id}/conversations/";
            var newItem = new IssueConversationDTO()
            {
                Type = IssueConversation.MessageType,
                Data = "Client was able to write a message.",
            };

            var response = await _client.PostAsync(uri, newItem.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [Test, Order(9)]
        // issue is in cancelled state after this test.
        public async Task CustomerCanceledIssueTestAsync()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyClientSignIn.Token);
            
            var project = await TestHelper.Instance.GetProject();

            issue.State = await TestHelper.Instance.GetStateByName(_client, project.Id, State.CancelledState);
            var res = await TestHelper.Instance.UpdateIssueAsync(_client, issue);

            Assert.AreEqual(HttpStatusCode.NoContent, res.Status);
        }

        [Test, Order(9)]
        // issue is in cancelled state after this test.
        public async Task CustomerCannotCancelInternalIssueTestAsync()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyClientSignIn.Token);

            var project = await TestHelper.Instance.GetProject();

            internalIssue.State = await TestHelper.Instance.GetStateByName(_client, project.Id, State.CancelledState);
            var res = await TestHelper.Instance.UpdateIssueAsync(_client, internalIssue);

            Assert.AreEqual(HttpStatusCode.Forbidden, res.Status);
        }

        [Test, Order(1)]
        public async Task CustomerShouldNotBeAbleToStartTimeSheetTestAsync()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyClientSignIn.Token);
            var uri = $"/api/issues/{issue.Id}/timesheets";

            var newItem = new IssueTimeSheetDTO()
            {
                User = companyClientSignIn.User,
                Start = DateTime.Now,
                End = DateTime.Now
            };

            var response = await _client.PostAsync(uri, newItem.ToStringContent());
            var r = await response.Content.Parse<IssueTimeSheetDTO>();
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, "Customer was able to create a timesheet!");
        }

        //[Test]
        //public void CustomerShouldNotBeAbleToEditTimeSheetTest()
        //{
        //    Assert.Fail();
        //}

        [Test, Order(1)]
        public async Task EmployeeCanWriteMessageTestAsync()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyEmployeeSignIn.Token);

            var uri = $"/api/issues/{issue.Id}/conversations/";
            var newItem = new IssueConversationDTO()
            {
                Type = IssueConversation.MessageType,
                Data = "Employee was able to write a message.",
            };

            var response = await _client.PostAsync(uri, newItem.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        //[Test]
        //public void EmployeeCanEditStateOfIssueTest()
        //{
        //    Assert.Fail();
        //}

        //[Test]
        //public void EmployeeCanEditOwnTimeSheetsOfIssueTest()
        //{
        //    Assert.Fail();
        //}

        //[Test]
        //public void EmployeeCanAddSubIssueTest()
        //{
        //    Assert.Fail();
        //}

        //[Test]
        //public void ProjectLeaderCanCanceleIssueTest()
        //{
        //    Assert.Fail();
        //}

        //[Test]
        //public void ProjectLeaderCanEditOtherTimeSheetsOfIssueTest()
        //{
        //    Assert.Fail();
        //}

        //[Test]
        //public void CompanyOwnerCanCanceleIssueTest()
        //{
        //    Assert.Fail();
        //}

        //[Test]
        //public void CompanyOwnerCanEditOtherTimeSheetsOfIssueTest()
        //{
        //    Assert.Fail();
        //}

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

            await TestHelper.Instance.GenerateIssue(_client, visibility: true);
            await TestHelper.Instance.GenerateIssue(_client, index: 1);

            // set issue phase to in edit.
            issue = await TestHelper.Instance.GetIssueDTOAsync(_client);
            issue.State = await TestHelper.Instance.GetStateByName(_client, (await TestHelper.Instance.GetProject()).Id, State.ProcessingState);
            issue = (await TestHelper.Instance.UpdateIssueAsync(_client, issue)).Response ?? issue;

            internalIssue = await TestHelper.Instance.GetIssueDTOAsync(_client, 1);
            internalIssue.State = await TestHelper.Instance.GetStateByName(_client, (await TestHelper.Instance.GetProject()).Id, State.ProcessingState);
            internalIssue = (await TestHelper.Instance.UpdateIssueAsync(_client, internalIssue, 1)).Response ?? internalIssue;
        }
    }
}
