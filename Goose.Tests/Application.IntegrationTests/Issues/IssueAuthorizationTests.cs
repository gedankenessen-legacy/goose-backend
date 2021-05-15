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
    [Parallelizable(ParallelScope.All)]
    public class IssueAuthorizationTests
    {
        private class SimpleTestHelperBuilderIssueAuthorization : SimpleTestHelperBuilder
        {
            private Role _role;

            public SimpleTestHelperBuilderIssueAuthorization(Role role)
            {
                _role = role;
            }

            public override IssueDTO GetIssueDTOCopy(HttpClient client, SimpleTestHelper helper)
            {
                IssueDTO issueCopy = base.GetIssueDTOCopy(client, helper);
                issueCopy.IssueDetail.RequirementsNeeded = false;
                issueCopy.IssueDetail.Visibility = true;
                return issueCopy;
            }

            public override async Task<SimpleTestHelper> Build()
            {
                var helper = await base.Build();

                await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, _role);

                return helper;
            }
        }
        #region T76

        [Test]
        public async Task CustomerCanWriteMessageTestAsync()
        {
            using var helper = await new SimpleTestHelperBuilderIssueAuthorization(Role.CustomerRole).Build();

            var uri = $"/api/issues/{helper.Issue.Id}/conversations/";
            var newItem = new IssueConversationDTO()
            {
                Type = IssueConversation.MessageType,
                Data = "Client was able to write a message.",
            };

            var response = await helper.client.PostAsync(uri, newItem.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [Test]
        // issue is in cancelled state after this test.
        public async Task CustomerCanceledIssueTestAsync()
        {
            using var helper = await new SimpleTestHelperBuilderIssueAuthorization(Role.CustomerRole).Build();

            var uri = $"/api/projects/{helper.Project.Id}/issues/{helper.Issue.Id}";
            helper.Issue.State = await helper.Helper.GetStateByNameAsync(helper.Project.Id, State.CancelledState);
            var res = await helper.client.PutAsync(uri, helper.Issue.ToStringContent());

            Assert.IsTrue(res.IsSuccessStatusCode);

            res = await helper.client.GetAsync(uri);
            IssueDTO updatedIssue = await res.Parse<IssueDTO>();
            Assert.AreEqual(updatedIssue.State.Name, State.CancelledState);
        }

        [Test]
        public async Task CustomerShouldNotBeAbleToStartTimeSheetTestAsync()
        {
            using var helper = await new SimpleTestHelperBuilderIssueAuthorization(Role.CustomerRole).Build();

            //_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyClientSignIn.Token);
            var uri = $"/api/issues/{helper.Issue.Id}/timesheets";

            var newItem = new IssueTimeSheetDTO()
            {
                User = helper.User,
                Start = DateTime.Now,
                End = DateTime.Now
            };

            var response = await helper.client.PostAsync(uri, newItem.ToStringContent());
            var r = await response.Content.Parse<IssueTimeSheetDTO>();
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, "Customer was able to create a timesheet!");
        }

        //[Test]
        //public void CustomerShouldNotBeAbleToEditTimeSheetTest()
        //{
        //    Assert.Fail();
        //}

        [Test]
        public async Task EmployeeCanWriteMessageTestAsync()
        {
            using var helper = await new SimpleTestHelperBuilderIssueAuthorization(Role.EmployeeRole).Build();

            var uri = $"/api/issues/{helper.Issue.Id}/conversations/";
            var newItem = new IssueConversationDTO()
            {
                Type = IssueConversation.MessageType,
                Data = "Employee was able to write a message.",
            };

            var response = await helper.client.PostAsync(uri, newItem.ToStringContent());
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

        //    private async Task<HttpResponseMessage> AssignUserToProjectAsync(string uri, PropertyUserDTO? propertyUser = null)
        //    {
        //        propertyUser ??= new PropertyUserDTO()
        //        {
        //            User = companyClientSignIn.User,
        //            Roles = new List<RoleDTO>() {
        //                    new RoleDTO (Role.CustomerRole)
        //            }
        //        };

        //        return await _client.PutAsync(uri, propertyUser.ToStringContent());
        //    }

        //    private async Task Generate()
        //    {
        //        companyOwnerSignIn = await TestHelper.Instance.GenerateCompany(_client);
        //        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyOwnerSignIn.Token);
        //        createdProject = await TestHelper.Instance.GenerateProject(_client);

        //        PropertyUserLoginDTO customerSignIn = new()
        //        {
        //            Firstname = "Test",
        //            Lastname = "Kunde",
        //            Password = "string1234",
        //            Roles = new List<RoleDTO>() {
        //                    new RoleDTO (Role.CustomerRole)
        //            }

        //        };

        //        PropertyUserLoginDTO employeeSignIn = new()
        //        {
        //            Firstname = "Test",
        //            Lastname = "Mitarbeiter",
        //            Password = "string1234",
        //            Roles = new List<RoleDTO>() {
        //                    new RoleDTO (Role.EmployeeRole)
        //            }
        //        };

        //        companyClientSignIn = await TestHelper.Instance.GenerateUserForCompany(_client, _companyRepository, customerSignIn);
        //        companyClientSignIn = await TestHelper.Instance.SignIn(_client, new() { Username = companyClientSignIn.User.Username, Password = customerSignIn.Password });

        //        companyEmployeeSignIn = await TestHelper.Instance.GenerateUserForCompany(_client, _companyRepository, employeeSignIn);
        //        companyEmployeeSignIn = await TestHelper.Instance.SignIn(_client, new() { Username = companyEmployeeSignIn.User.Username, Password = employeeSignIn.Password });

        //        await AssignUserToProjectAsync($"api/projects/{createdProject.Id}/users/{companyClientSignIn.User.Id}", new()
        //        {
        //            User = new() { Id = companyClientSignIn.User.Id },
        //            Roles = new List<RoleDTO>() {
        //                    new RoleDTO (Role.CustomerRole)
        //            }
        //        });

        //        await AssignUserToProjectAsync($"api/projects/{createdProject.Id}/users/{companyEmployeeSignIn.User.Id}", new()
        //        {
        //            User = new() { Id = companyEmployeeSignIn.User.Id },
        //            Roles = new List<RoleDTO>() {
        //                    new RoleDTO (Role.EmployeeRole)
        //            }
        //        });

        //        await TestHelper.Instance.GenerateIssue(_client);

        //        // set issue phase to in edit.
        //        issue = await TestHelper.Instance.GetIssueDTOAsync(_client);
        //        issue.State = await TestHelper.Instance.GetStateByName(_client, (await TestHelper.Instance.GetProject()).Id, State.ProcessingState);
        //        issue = (await TestHelper.Instance.UpdateIssueAsync(_client, issue)).Response ?? issue;
        //    }
    }
}
