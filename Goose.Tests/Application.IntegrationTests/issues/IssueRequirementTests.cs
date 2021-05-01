using Goose.API;
using Goose.API.Repositories;
using Goose.Domain.Models.Auth;
using Goose.Domain.Models.Issues;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    [TestFixture]
    [SingleThreaded]
    class IssueRequirementTests
    {
        private HttpClient _client;

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
        public async Task AddRequirement()
        {
            var issue = await TestHelper.Instance.GetIssueAsync();
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            var uri = $"/api/issues/{issue.Id}/requirements/";
            var responce = await _client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            var exits = issue.IssueDetail.Requirements?.FirstOrDefault(x => x.Requirement.Equals("Die Application Testen")) != null;
            Assert.IsTrue(exits);
        }

        [Test]
        public async Task AddRequirementFalse()
        {
            var issue = await TestHelper.Instance.GetIssueAsync();
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = " " };
            var uri = $"/api/issues/{issue.Id}/requirements/";
            var responce = await _client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsFalse(responce.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            var exits = issue.IssueDetail.Requirements?.FirstOrDefault(x => x.Requirement.Equals("Die Application Testen")) != null;
            Assert.IsFalse(exits);
        }

        [Test]
        public async Task DeleteRequirement()
        {
            var issue = await TestHelper.Instance.GetIssueAsync();
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            var uri = $"/api/issues/{issue.Id}/requirements/";
            var responce = await _client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            var addedRequirement = issue.IssueDetail.Requirements?.FirstOrDefault(x => x.Requirement.Equals("Die Application Testen"));
            uri = $"/api/issues/{issue.Id}/requirements/{addedRequirement.Id}";
            responce = await _client.DeleteAsync(uri);
            Assert.IsTrue(responce.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            var exits = issue.IssueDetail.Requirements?.FirstOrDefault(x => x.Requirement.Equals("Die Application Testen")) != null;
            Assert.IsFalse(exits);
        }
    }
}
