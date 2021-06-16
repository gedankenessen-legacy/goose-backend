using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Auth;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Issues;
using Goose.Domain.Models.Projects;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Tests.Application.IntegrationTests.Issues
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    class IssueSummaryRightTests
    {
        private class SimpleTestHelperBuilderSummaryRights : SimpleTestHelperBuilder
        {
            public UserDTO Costumer { get; set; }
            private Role _role;

            public SimpleTestHelperBuilderSummaryRights(Role role)
            {
                _role = role;
            }

            public override async Task<SimpleTestHelper> Build()
            {
                var helper = await base.Build();
                var customer = await helper.GenerateUserAndSetToProject(_role);
                Costumer = new UserDTO(await helper.Helper.UserRepository.GetAsync(customer.Id));
                var issue = base.GetIssueDTOCopy(helper.client, helper);
                issue.Author = Costumer;
                issue.IssueDetail.Visibility = true;
                var responce = await helper.CreateIssue(issue);
                helper.Issue = await responce.Parse<IssueDTO>();
                helper.Helper.SetAuth(helper.SignIn);
                return helper;
            }

            public override Task<IssueDTO> CreateIssue(HttpClient client, SimpleTestHelper helper) => null;

        }

        private class SimpleTestHelperBuilderSummaryRights2 : SimpleTestHelperBuilder
        {
            public UserDTO Employee { get; set; }
            public UserDTO Customer { get; set; }

            public SimpleTestHelperBuilderSummaryRights2()
            {
            }

            public override async Task<SimpleTestHelper> Build()
            {
                var helper = await base.Build();
                var customer = await helper.GenerateUserAndSetToProject(Role.CustomerRole);
                Customer = new UserDTO(await helper.Helper.UserRepository.GetAsync(customer.Id));
                var employee = await helper.GenerateUserAndSetToProject(Role.EmployeeRole);
                Employee = new UserDTO(await helper.Helper.UserRepository.GetAsync(employee.Id));
                var issue = base.GetIssueDTOCopy(helper.client, helper);
                issue.Author = Employee;
                issue.Client = Customer;
                issue.IssueDetail.Visibility = true;
                var responce = await helper.CreateIssue(issue);
                helper.Issue = await responce.Parse<IssueDTO>();
                helper.Helper.SetAuth(helper.SignIn);
                return helper;
            }

            public override Task<IssueDTO> CreateIssue(HttpClient client, SimpleTestHelper helper) => null;

        }


        [Test]
        public async Task CreateSummary1()
        {
            using var helper = await new SimpleTestHelperBuilderSummaryRights(Role.CustomerRole).Build();

            var issue = helper.Issue;
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            await helper.Helper.IssueRequirementService.CreateAsync(issue.Id, issueRequirement);

            var uri = $"/api/issues/{issue.Id}/summaries";
            var response = await helper.client.PostAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            uri = $"/api/issues/{issue.Id}/summaries";
            response = await helper.client.GetAsync(uri);
            Assert.IsTrue(response.IsSuccessStatusCode);

            var requirements = await response.Content.Parse<IList<IssueRequirement>>();

            Assert.IsTrue(requirements != null && requirements.Count > 0);

            issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen2" };
            uri = $"/api/issues/{issue.Id}/requirements/";
            response = await helper.client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsFalse(response.IsSuccessStatusCode);

        }

        [Test]
        public async Task CreateSummary2()
        {
            var builder = new SimpleTestHelperBuilderSummaryRights(Role.CustomerRole);
            using var helper = await builder.Build();

            var signIn = await helper.Helper.SignIn(new SignInRequest()
            {
                Username = builder.Costumer.Username,
                Password = helper.Helper.UsedPasswordForTests
            });

            helper.Helper.SetAuth(signIn);

            var issue = helper.Issue;
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            await helper.Helper.IssueRequirementService.CreateAsync(issue.Id, issueRequirement);

            var uri = $"/api/issues/{issue.Id}/summaries";
            var response = await helper.client.PostAsync(uri, 1.0.ToStringContent());
            Assert.IsFalse(response.IsSuccessStatusCode);
        }

        [Test]
        public async Task AcceptSummary1()
        {
            var builder = new SimpleTestHelperBuilderSummaryRights(Role.CustomerRole);
            using var helper = await builder.Build();
            await helper.SetState(State.NegotiationState);

            var issue = helper.Issue;
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            await helper.Helper.IssueRequirementService.CreateAsync(issue.Id, issueRequirement);

            var uri = $"/api/issues/{issue.Id}/summaries";
            var response = await helper.client.PostAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            var signIn = await helper.Helper.SignIn(new SignInRequest()
            {
                Username = builder.Costumer.Username,
                Password = helper.Helper.UsedPasswordForTests
            });

            helper.Helper.SetAuth(signIn);

            uri = $"/api/issues/{issue.Id}/summaries?accept=true";
            response = await helper.client.PutAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            var newIssue = await helper.GetIssueAsync(issue.Id);
            Assert.IsTrue(newIssue.IssueDetail.RequirementsAccepted);

            Assert.AreEqual(State.ProcessingState, (await helper.Helper.GetStateById(newIssue)).Name);

            issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen2" };
            uri = $"/api/issues/{issue.Id}/requirements/";
            response = await helper.client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsFalse(response.IsSuccessStatusCode);

        }

        [Test]
        public async Task AcceptSummary2()
        {
            var builder = new SimpleTestHelperBuilderSummaryRights(Role.CustomerRole);
            using var helper = await builder.Build();

            var issue = helper.Issue;
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            await helper.Helper.IssueRequirementService.CreateAsync(issue.Id, issueRequirement);

            var uri = $"/api/issues/{issue.Id}/summaries";
            var response = await helper.client.PostAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            uri = $"/api/issues/{issue.Id}/summaries?accept=true";
            response = await helper.client.PutAsync(uri, 1.0.ToStringContent());
            Assert.IsFalse(response.IsSuccessStatusCode);
        }

        [Test]
        public async Task AcceptSummary3()
        {
            var builder = new SimpleTestHelperBuilderSummaryRights(Role.EmployeeRole);
            using var helper = await builder.Build();
            await helper.SetState(State.NegotiationState);
            
            var issue = helper.Issue;
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            await helper.Helper.IssueRequirementService.CreateAsync(issue.Id, issueRequirement);

            var uri = $"/api/issues/{issue.Id}/summaries";
            var response = await helper.client.PostAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            var signIn = await helper.Helper.SignIn(new SignInRequest()
            {
                Username = builder.Costumer.Username,
                Password = helper.Helper.UsedPasswordForTests
            });

            helper.Helper.SetAuth(signIn);

            uri = $"/api/issues/{issue.Id}/summaries?accept=true";
            response = await helper.client.PutAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            var newIssue = await helper.GetIssueAsync(issue.Id);
            Assert.IsTrue(newIssue.IssueDetail.RequirementsAccepted);

            Assert.AreEqual(State.ProcessingState, (await helper.Helper.GetStateById(newIssue)).Name);

            issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen2" };
            uri = $"/api/issues/{issue.Id}/requirements/";
            response = await helper.client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsFalse(response.IsSuccessStatusCode);

        }

        [Test]
        public async Task AcceptSummary4()
        {
            var builder = new SimpleTestHelperBuilderSummaryRights(Role.EmployeeRole);
            using var helper = await builder.Build();
            await helper.SetState(State.NegotiationState);

            var issue = helper.Issue;
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            await helper.Helper.IssueRequirementService.CreateAsync(issue.Id, issueRequirement);

            var uri = $"/api/issues/{issue.Id}/summaries";
            var response = await helper.client.PostAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            uri = $"/api/issues/{issue.Id}/summaries?accept=true";
            response = await helper.client.PutAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            var newIssue = await helper.GetIssueAsync(issue.Id);
            Assert.IsTrue(newIssue.IssueDetail.RequirementsAccepted);

            Assert.AreEqual(State.ProcessingState, (await helper.Helper.GetStateById(newIssue)).Name);

            issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen2" };
            uri = $"/api/issues/{issue.Id}/requirements/";
            response = await helper.client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsFalse(response.IsSuccessStatusCode);

        }

        [Test]
        public async Task AcceptSummary5()
        {
            var builder = new SimpleTestHelperBuilderSummaryRights2();
            using var helper = await builder.Build();
            await helper.SetState(State.NegotiationState);

            var issue = helper.Issue;
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            await helper.Helper.IssueRequirementService.CreateAsync(issue.Id, issueRequirement);

            var uri = $"/api/issues/{issue.Id}/summaries";
            var response = await helper.client.PostAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            var signIn = await helper.Helper.SignIn(new SignInRequest()
            {
                Username = builder.Customer.Username,
                Password = helper.Helper.UsedPasswordForTests
            });

            helper.Helper.SetAuth(signIn);

            uri = $"/api/issues/{issue.Id}/summaries?accept=true";
            response = await helper.client.PutAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            var newIssue = await helper.GetIssueAsync(issue.Id);
            Assert.IsTrue(newIssue.IssueDetail.RequirementsAccepted);

            Assert.AreEqual(State.ProcessingState, (await helper.Helper.GetStateById(newIssue)).Name);

            issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen2" };
            uri = $"/api/issues/{issue.Id}/requirements/";
            response = await helper.client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsFalse(response.IsSuccessStatusCode);

        }

        [Test]
        public async Task DeclineSummary1()
        {
            var builder = new SimpleTestHelperBuilderSummaryRights(Role.CustomerRole);
            using var helper = await builder.Build();

            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            await helper.Helper.IssueRequirementService.CreateAsync(helper.Issue.Id, issueRequirement);

            var uri = $"/api/issues/{helper.Issue.Id}/summaries";
            var responce = await helper.client.PostAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            var customerSignIn = await helper.Helper.SignIn(new SignInRequest()
            {
                Username = builder.Costumer.Username,
                Password = helper.Helper.UsedPasswordForTests
            });

            helper.Helper.SetAuth(customerSignIn);

            uri = $"/api/issues/{helper.Issue.Id}/summaries?accept=false";
            responce = await helper.client.PutAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            var issue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.IsFalse(issue.IssueDetail.RequirementsAccepted);
            Assert.IsFalse(issue.IssueDetail.RequirementsSummaryCreated);

            // Client is not allowed to post requirements!
            helper.Helper.SetAuth(helper.SignIn);

            issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen2" };
            uri = $"/api/issues/{issue.Id}/requirements/";
            responce = await helper.client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);
        }

        [Test]
        public async Task DeclineSummary2()
        {
            var builder = new SimpleTestHelperBuilderSummaryRights(Role.CustomerRole);
            using var helper = await builder.Build();

            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            await helper.Helper.IssueRequirementService.CreateAsync(helper.Issue.Id, issueRequirement);

            var uri = $"/api/issues/{helper.Issue.Id}/summaries";
            var responce = await helper.client.PostAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            uri = $"/api/issues/{helper.Issue.Id}/summaries?accept=false";
            responce = await helper.client.PutAsync(uri, 1.0.ToStringContent());
            Assert.IsFalse(responce.IsSuccessStatusCode);

            var issue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.IsFalse(issue.IssueDetail.RequirementsAccepted);
            Assert.IsTrue(issue.IssueDetail.RequirementsSummaryCreated);

            issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen2" };
            uri = $"/api/issues/{issue.Id}/requirements/";
            responce = await helper.client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsFalse(responce.IsSuccessStatusCode);

        }

        [Test]
        public async Task DeclineSummary3()
        {
            var builder = new SimpleTestHelperBuilderSummaryRights(Role.EmployeeRole);
            using var helper = await builder.Build();

            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            await helper.Helper.IssueRequirementService.CreateAsync(helper.Issue.Id, issueRequirement);

            var uri = $"/api/issues/{helper.Issue.Id}/summaries";
            var responce = await helper.client.PostAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            var signIn = await helper.Helper.SignIn(new SignInRequest()
            {
                Username = builder.Costumer.Username,
                Password = helper.Helper.UsedPasswordForTests
            });

            helper.Helper.SetAuth(signIn);

            uri = $"/api/issues/{helper.Issue.Id}/summaries?accept=false";
            responce = await helper.client.PutAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            var issue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.IsFalse(issue.IssueDetail.RequirementsAccepted);
            Assert.IsFalse(issue.IssueDetail.RequirementsSummaryCreated);

            issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen2" };
            uri = $"/api/issues/{issue.Id}/requirements/";
            responce = await helper.client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

        }

        [Test]
        public async Task DeclineSummary4()
        {
            var builder = new SimpleTestHelperBuilderSummaryRights(Role.EmployeeRole);
            using var helper = await builder.Build();

            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            await helper.Helper.IssueRequirementService.CreateAsync(helper.Issue.Id, issueRequirement);

            var uri = $"/api/issues/{helper.Issue.Id}/summaries";
            var responce = await helper.client.PostAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            uri = $"/api/issues/{helper.Issue.Id}/summaries?accept=false";
            responce = await helper.client.PutAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            var issue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.IsFalse(issue.IssueDetail.RequirementsAccepted);
            Assert.IsFalse(issue.IssueDetail.RequirementsSummaryCreated);

            issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen2" };
            uri = $"/api/issues/{issue.Id}/requirements/";
            responce = await helper.client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

        }

        [Test]
        public async Task DeclineSummary5()
        {
            var builder = new SimpleTestHelperBuilderSummaryRights2();
            using var helper = await builder.Build();
            await helper.SetState(State.NegotiationState);

            var issue = helper.Issue;
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            await helper.Helper.IssueRequirementService.CreateAsync(issue.Id, issueRequirement);

            var uri = $"/api/issues/{issue.Id}/summaries";
            var response = await helper.client.PostAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            var signIn = await helper.Helper.SignIn(new SignInRequest()
            {
                Username = builder.Customer.Username,
                Password = helper.Helper.UsedPasswordForTests
            });

            helper.Helper.SetAuth(signIn);

            uri = $"/api/issues/{issue.Id}/summaries?accept=false";
            response = await helper.client.PutAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            var issueUpdated = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.IsFalse(issueUpdated.IssueDetail.RequirementsAccepted);
            Assert.IsFalse(issueUpdated.IssueDetail.RequirementsSummaryCreated);

            helper.Helper.SetAuth(helper.SignIn);

            issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen2" };
            uri = $"/api/issues/{issue.Id}/requirements/";
            response = await helper.client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

        }
    }
}
