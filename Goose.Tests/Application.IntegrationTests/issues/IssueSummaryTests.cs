using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Goose.API;
using Goose.API.Services.Issues;
using Goose.Domain.Models.Issues;
using Goose.Domain.Models.Projects;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    class IssueSummaryTests
    {
        [Test]
        public async Task CreateSummary()
        {
            using (var scope = new TestScope())
            {
                var issue = scope.Helper.Issue;
                IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
                await scope.Helper.Helper.IssueRequirementService.CreateAsync(issue.Id, issueRequirement);

                var uri = $"/api/issues/{issue.Id}/summaries";
                var response = await scope.client.PostAsync(uri, new object().ToStringContent());
                Assert.IsTrue(response.IsSuccessStatusCode);

                uri = $"/api/issues/{issue.Id}/summaries";
                response = await scope.client.GetAsync(uri);
                Assert.IsTrue(response.IsSuccessStatusCode);

                var requirements = await response.Content.Parse<IList<IssueRequirement>>();

                Assert.IsTrue(requirements != null && requirements.Count > 0);

                issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen2" };
                uri = $"/api/issues/{issue.Id}/requirements/";
                response = await scope.client.PostAsync(uri, issueRequirement.ToStringContent());
                Assert.IsFalse(response.IsSuccessStatusCode);
            }
        }

        [Test]
        public async Task CreateSummaryFalse()
        {
            using (var scope = new TestScope())
            {
                var issue = scope.Helper.Issue;

                var uri = $"/api/issues/{issue.Id}/summaries";
                var responce = await scope.client.PostAsync(uri, new object().ToStringContent());
                Assert.IsFalse(responce.IsSuccessStatusCode);

                uri = $"/api/issues/{issue.Id}/summaries";
                responce = await scope.client.GetAsync(uri);
                Assert.IsFalse(responce.IsSuccessStatusCode);
            }
        }

        [Test]
        public async Task AcceptSummary()
        {
            using (var scope = new TestScope())
            {
                var issue = scope.Helper.Issue;
                IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
                await scope.Helper.Helper.IssueRequirementService.CreateAsync(issue.Id, issueRequirement);

                var uri = $"/api/issues/{issue.Id}/summaries";
                var response = await scope.client.PostAsync(uri, new object().ToStringContent());
                Assert.IsTrue(response.IsSuccessStatusCode);

                uri = $"/api/issues/{issue.Id}/summaries?accept=true";
                response = await scope.client.PutAsync(uri, new object().ToStringContent());
                Assert.IsTrue(response.IsSuccessStatusCode);

                var newIssue = await scope.Helper.GetIssueAsync(issue.Id);
                Assert.IsTrue(newIssue.IssueDetail.RequirementsAccepted);

                var state = await scope.Helper.Helper.GetStateByNameAsync(newIssue.ProjectId, State.WaitingState);
                Assert.AreEqual(state.Id, newIssue.StateId);

                issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen2" };
                uri = $"/api/issues/{issue.Id}/requirements/";
                response = await scope.client.PostAsync(uri, issueRequirement.ToStringContent());
                Assert.IsFalse(response.IsSuccessStatusCode);
            }
        }

        [Test]
        public async Task AcceptSummaryFalse()
        {
            using (var scope = new TestScope())
            {
                var uri = $"/api/issues/{scope.Helper.Issue.Id}/summaries?accept=true";
                var response = await scope.client.PutAsync(uri, new object().ToStringContent());
                Assert.IsFalse(response.IsSuccessStatusCode);
            }
        }

        [Test]
        public async Task DeclineSummary()
        {
            using (var scope = new TestScope())
            {
                IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
                await scope.Helper.Helper.IssueRequirementService.CreateAsync(scope.Helper.Issue.Id, issueRequirement);

                var uri = $"/api/issues/{scope.Helper.Issue.Id}/summaries";
                var responce = await scope.client.PostAsync(uri, new object().ToStringContent());
                Assert.IsTrue(responce.IsSuccessStatusCode);

                uri = $"/api/issues/{scope.Helper.Issue.Id}/summaries?accept=false";
                responce = await scope.client.PutAsync(uri, new object().ToStringContent());
                Assert.IsTrue(responce.IsSuccessStatusCode);

                var issue = await scope.Helper.GetIssueAsync(scope.Helper.Issue.Id);
                Assert.IsFalse(issue.IssueDetail.RequirementsAccepted);
                Assert.IsFalse(issue.IssueDetail.RequirementsSummaryCreated);

                issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen2" };
                uri = $"/api/issues/{issue.Id}/requirements/";
                responce = await scope.client.PostAsync(uri, issueRequirement.ToStringContent());
                Assert.IsTrue(responce.IsSuccessStatusCode);
            }
            
        }

        [Test]
        public async Task DeclineSummaryFalse()
        {
            using (var scope = new TestScope())
            {
                var uri = $"/api/issues/{scope.Helper.Issue.Id}/summaries?accept=false";
                var responce = await scope.client.PutAsync(uri, new object().ToStringContent());
                Assert.IsFalse(responce.IsSuccessStatusCode);

                var issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen2" };
                uri = $"/api/issues/{scope.Helper.Issue.Id}/requirements/";
                responce = await scope.client.PostAsync(uri, issueRequirement.ToStringContent());
                Assert.IsTrue(responce.IsSuccessStatusCode);
            }
        }

        [Test]
        public async Task DeclineSummaryFalse2()
        {
            using (var scope = new TestScope())
            {
                IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
                await scope.Helper.Helper.IssueRequirementService.CreateAsync(scope.Helper.Issue.Id, issueRequirement);

                var uri = $"/api/issues/{scope.Helper.Issue.Id}/summaries";
                var responce = await scope.client.PostAsync(uri, new object().ToStringContent());
                Assert.IsTrue(responce.IsSuccessStatusCode);

                uri = $"/api/issues/{scope.Helper.Issue.Id}/summaries?accept=true";
                responce = await scope.client.PutAsync(uri, new object().ToStringContent());
                Assert.IsTrue(responce.IsSuccessStatusCode);

                var issue = await scope.Helper.GetIssueAsync(scope.Helper.Issue.Id);
                Assert.IsTrue(issue.IssueDetail.RequirementsAccepted);

                
                var state = await scope.Helper.Helper.GetStateByNameAsync(issue.ProjectId, State.WaitingState);
                Assert.AreEqual(state.Id, issue.StateId);

                uri = $"/api/issues/{issue.Id}/summaries?accept=false";
                responce = await scope.client.PutAsync(uri, new object().ToStringContent()); 
                Assert.IsFalse(responce.IsSuccessStatusCode);
            }
        }
    }
}
