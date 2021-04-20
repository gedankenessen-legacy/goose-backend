using Goose.API;
using Goose.API.Repositories;
using Goose.API.Services.Issues;
using Goose.Domain.Models.Auth;
using Goose.Domain.Models.Projects;
using Goose.Domain.Models.Issues;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    [TestFixture]
    class IssueSummaryTests
    {
        private HttpClient _client;
        private IIssueRequirementService _issueRequirementService;
        private SignInResponse signInObject;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var factory = new WebApplicationFactory<Startup>();
            _client = factory.CreateClient();
        }

        [SetUp]
        public async Task Setup()
        {
            await TestHelper.Instance.GenerateAll(_client);
        }

        [TearDown]
        public async Task TearDown()
        {
            await TestHelper.Instance.ClearAll();
        }

        [Test]
        public async Task CreateSummary()
        {
            var issue = await TestHelper.Instance.GetIssueAsync();
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            await _issueRequirementService.CreateAsync(issue.Id, issueRequirement);

            var uri = $"/api/issues/{issue.Id}/summaries";
            var responce = await _client.PostAsync(uri, new object().ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            uri = $"/api/issues/{issue.Id}/summaries";
            responce = await _client.GetAsync(uri);
            Assert.IsTrue(responce.IsSuccessStatusCode);

            var requirements = await responce.Content.Parse<IList<IssueRequirement>>();

            Assert.IsTrue(requirements != null && requirements.Count > 0);

            issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen2" };
            uri = $"/api/issues/{issue.Id}/requirements/";
            responce = await _client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsFalse(responce.IsSuccessStatusCode);
        }

        [Test]
        public async Task CreateSummaryFalse()
        {
            var issue = await TestHelper.Instance.GetIssueAsync();

            var uri = $"/api/issues/{issue.Id}/summaries";
            var responce = await _client.PostAsync(uri, new object().ToStringContent());
            Assert.IsFalse(responce.IsSuccessStatusCode);

            uri = $"/api/issues/{issue.Id}/summaries";
            responce = await _client.GetAsync(uri);
            Assert.IsFalse(responce.IsSuccessStatusCode);
        }

        [Test]
        public async Task AcceptSummary()
        {
            var issue = await TestHelper.Instance.GetIssueAsync();
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            await _issueRequirementService.CreateAsync(issue.Id, issueRequirement);

            var uri = $"/api/issues/{issue.Id}/summaries";
            var responce = await _client.PostAsync(uri, new object().ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            uri = $"/api/issues/{issue.Id}/summaries?accept=true";
            responce = await _client.PutAsync(uri, new object().ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            Assert.IsTrue(issue.IssueDetail.RequirementsAccepted);

            var state = await TestHelper.Instance.GetStateByName(_client, issue.ProjectId, State.WaitingState);
            Assert.AreEqual(state.Id, issue.StateId);

            issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen2" };
            uri = $"/api/issues/{issue.Id}/requirements/";
            responce = await _client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsFalse(responce.IsSuccessStatusCode);
        }

        [Test]
        public async Task AcceptSummaryFalse()
        {
            var issue = await TestHelper.Instance.GetIssueAsync();
            var uri = $"/api/issues/{issue.Id}/summaries?accept=true";
            var responce = await _client.PutAsync(uri, new object().ToStringContent());
            Assert.IsFalse(responce.IsSuccessStatusCode);
        }

        [Test]
        public async Task DeclineSummary()
        {
            var issue = await TestHelper.Instance.GetIssueAsync();
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            await _issueRequirementService.CreateAsync(issue.Id, issueRequirement);

            var uri = $"/api/issues/{issue.Id}/summaries";
            var responce = await _client.PostAsync(uri, new object().ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            uri = $"/api/issues/{issue.Id}/summaries?accept=false";
            responce = await _client.PutAsync(uri, new object().ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            Assert.IsFalse(issue.IssueDetail.RequirementsAccepted);
            Assert.IsFalse(issue.IssueDetail.RequirementsSummaryCreated);

            issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen2" };
            uri = $"/api/issues/{issue.Id}/requirements/";
            responce = await _client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);
        }

        [Test]
        public async Task DeclineSummaryFalse()
        {
            var issue = await TestHelper.Instance.GetIssueAsync();

            var uri = $"/api/issues/{issue.Id}/summaries?accept=false";
            var responce = await _client.PutAsync(uri, new object().ToStringContent());
            Assert.IsFalse(responce.IsSuccessStatusCode);

            var issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen2" };
            uri = $"/api/issues/{issue.Id}/requirements/";
            responce = await _client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);
        }

        [Test]
        public async Task DeclineSummaryFalse2()
        {
            var issue = await TestHelper.Instance.GetIssueAsync();
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            await _issueRequirementService.CreateAsync(issue.Id, issueRequirement);

            var uri = $"/api/issues/{issue.Id}/summaries";
            var responce = await _client.PostAsync(uri, new object().ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            uri = $"/api/issues/{issue.Id}/summaries?accept=true";
            responce = await _client.PutAsync(uri, new object().ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            Assert.IsTrue(issue.IssueDetail.RequirementsAccepted);

            var state = await TestHelper.Instance.GetStateByName(_client, issue.ProjectId, State.WaitingState);
            Assert.AreEqual(state.Id, issue.StateId);

            uri = $"/api/issues/{issue.Id}/summaries?accept=false";
            responce = await _client.PutAsync(uri, new object().ToStringContent()); 
            Assert.IsFalse(responce.IsSuccessStatusCode);
        }
    }
}
