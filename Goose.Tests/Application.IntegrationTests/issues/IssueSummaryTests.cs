using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Auth;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Issues;
using Goose.Domain.Models.Projects;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.Issues
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    class IssueSummaryTests
    {
        [Test]
        public async Task CreateSummary()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();

            var issue = helper.Issue;
            IssueRequirement issueRequirement = new IssueRequirement() {Requirement = "Die Application Testen"};
            await helper.Helper.IssueRequirementService.CreateAsync(issue.Id, issueRequirement);

            var uri = $"/api/issues/{issue.Id}/summaries";
            var response = await helper.client.PostAsync(uri, new object().ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            uri = $"/api/issues/{issue.Id}/summaries";
            response = await helper.client.GetAsync(uri);
            Assert.IsTrue(response.IsSuccessStatusCode);

            var requirements = await response.Content.Parse<IList<IssueRequirement>>();

            Assert.IsTrue(requirements != null && requirements.Count > 0);

            issueRequirement = new IssueRequirement() {Requirement = "Die Application Testen2"};
            uri = $"/api/issues/{issue.Id}/requirements/";
            response = await helper.client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsFalse(response.IsSuccessStatusCode);
        }

        [Test]
        public async Task CreateSummaryFalse()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();

            var issue = helper.Issue;

            var uri = $"/api/issues/{issue.Id}/summaries";
            var responce = await helper.client.PostAsync(uri, new object().ToStringContent());
            Assert.IsFalse(responce.IsSuccessStatusCode);

            uri = $"/api/issues/{issue.Id}/summaries";
            responce = await helper.client.GetAsync(uri);
            Assert.IsFalse(responce.IsSuccessStatusCode);
        }

        [Test]
        public async Task AcceptSummary()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            await helper.SetState(State.NegotiationState);

            var issue = helper.Issue;
            IssueRequirement issueRequirement = new IssueRequirement() {Requirement = "Die Application Testen"};
            await helper.Helper.IssueRequirementService.CreateAsync(issue.Id, issueRequirement);

            var uri = $"/api/issues/{issue.Id}/summaries";
            var response = await helper.client.PostAsync(uri, new object().ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            uri = $"/api/issues/{issue.Id}/summaries?accept=true";
            response = await helper.client.PutAsync(uri, new object().ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            var newIssue = await helper.GetIssueAsync(issue.Id);
            Assert.IsTrue(newIssue.IssueDetail.RequirementsAccepted);

            Assert.AreEqual(State.ProcessingState, (await helper.Helper.GetStateById(newIssue)).Name);

            issueRequirement = new IssueRequirement() {Requirement = "Die Application Testen2"};
            uri = $"/api/issues/{issue.Id}/requirements/";
            response = await helper.client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsFalse(response.IsSuccessStatusCode);
        }

        [Test]
        public async Task AcceptSummaryFalse()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();

            var uri = $"/api/issues/{helper.Issue.Id}/summaries?accept=true";
            var response = await helper.client.PutAsync(uri, new object().ToStringContent());
            Assert.IsFalse(response.IsSuccessStatusCode);
        }

        [Test]
        public async Task DeclineSummary()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();

            IssueRequirement issueRequirement = new IssueRequirement() {Requirement = "Die Application Testen"};
            await helper.Helper.IssueRequirementService.CreateAsync(helper.Issue.Id, issueRequirement);

            var uri = $"/api/issues/{helper.Issue.Id}/summaries";
            var responce = await helper.client.PostAsync(uri, new object().ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            uri = $"/api/issues/{helper.Issue.Id}/summaries?accept=false";
            responce = await helper.client.PutAsync(uri, new object().ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            var issue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.IsFalse(issue.IssueDetail.RequirementsAccepted);
            Assert.IsFalse(issue.IssueDetail.RequirementsSummaryCreated);

            issueRequirement = new IssueRequirement() {Requirement = "Die Application Testen2"};
            uri = $"/api/issues/{issue.Id}/requirements/";
            responce = await helper.client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);
        }

        [Test]
        public async Task DeclineSummaryFalse()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();

            var uri = $"/api/issues/{helper.Issue.Id}/summaries?accept=false";
            var responce = await helper.client.PutAsync(uri, new object().ToStringContent());
            Assert.IsFalse(responce.IsSuccessStatusCode);

            var issueRequirement = new IssueRequirement() {Requirement = "Die Application Testen2"};
            uri = $"/api/issues/{helper.Issue.Id}/requirements/";
            responce = await helper.client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);
        }

        [Test]
        public async Task DeclineSummaryFalse2()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            await helper.SetState(State.NegotiationState);
            
            IssueRequirement issueRequirement = new IssueRequirement() {Requirement = "Die Application Testen"};
            await helper.Helper.IssueRequirementService.CreateAsync(helper.Issue.Id, issueRequirement);

            var uri = $"/api/issues/{helper.Issue.Id}/summaries";
            var responce = await helper.client.PostAsync(uri, new object().ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            uri = $"/api/issues/{helper.Issue.Id}/summaries?accept=true";
            responce = await helper.client.PutAsync(uri, new object().ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            var issue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.IsTrue(issue.IssueDetail.RequirementsAccepted);


            Assert.AreEqual(State.ProcessingState, (await helper.Helper.GetStateById(issue)).Name);

            uri = $"/api/issues/{issue.Id}/summaries?accept=false";
            responce = await helper.client.PutAsync(uri, new object().ToStringContent());
            Assert.IsFalse(responce.IsSuccessStatusCode);
        }
    }
}