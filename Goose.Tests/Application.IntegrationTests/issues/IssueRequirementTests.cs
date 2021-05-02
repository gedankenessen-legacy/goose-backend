using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Goose.API;
using Goose.Domain.Models.Issues;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    class IssueRequirementTests
    {
        private sealed class TestScope : IDisposable
        {
            public HttpClient client;
            public WebApplicationFactory<Startup> _factory;
            public SimpleTestHelper Helper;

            public TestScope()
            {
                Task.Run(() =>
                {
                    _factory = new WebApplicationFactory<Startup>();
                    client = _factory.CreateClient();
                    Helper = new SimpleTestHelperBuilder(client).Build().Result;
                    ;
                }).Wait();
            }

            public void Dispose()
            {
                client?.Dispose();
                _factory?.Dispose();
                Helper.Dispose();
            }
        }

        [Test]
        public async Task AddRequirement()
        {
            using (var scope = new TestScope())
            {
                IssueRequirement issueRequirement = new IssueRequirement() {Requirement = "Die Application Testen"};
                var uri = $"/api/issues/{scope.Helper.Issue.Id}/requirements/";
                var response = await scope.client.PostAsync(uri, issueRequirement.ToStringContent());
                Assert.IsTrue(response.IsSuccessStatusCode);

                var issue = await scope.Helper.GetIssueAsync(scope.Helper.Issue.Id);
                var exits = issue.IssueDetail.Requirements?.FirstOrDefault(x => x.Requirement.Equals("Die Application Testen")) != null;
                Assert.IsTrue(exits);
            }
        }

        [Test]
        public async Task AddRequirementFalse()
        {
            using (var scope = new TestScope())
            {
                IssueRequirement issueRequirement = new IssueRequirement() {Requirement = " "};
                var uri = $"/api/issues/{scope.Helper.Issue.Id}/requirements/";
                var responce = await scope.client.PostAsync(uri, issueRequirement.ToStringContent());
                Assert.IsFalse(responce.IsSuccessStatusCode);

                var issue = await scope.Helper.GetIssueAsync(scope.Helper.Issue.Id);
                var exits = issue.IssueDetail.Requirements?.FirstOrDefault(x => x.Requirement.Equals("Die Application Testen")) != null;
                Assert.IsFalse(exits);
            }
        }

        [Test]
        public async Task DeleteRequirement()
        {
            using (var scope = new TestScope())
            {
                IssueRequirement issueRequirement = new IssueRequirement() {Requirement = "Die Application Testen"};
                var uri = $"/api/issues/{scope.Helper.Issue.Id}/requirements/";
                var response = await scope.client.PostAsync(uri, issueRequirement.ToStringContent());
                Assert.IsTrue(response.IsSuccessStatusCode);

                var issue = await scope.Helper.GetIssueAsync(scope.Helper.Issue.Id);
                var addedRequirement = issue.IssueDetail.Requirements?.FirstOrDefault(x => x.Requirement.Equals("Die Application Testen"));
                uri = $"/api/issues/{issue.Id}/requirements/{addedRequirement.Id}";
                response = await scope.client.DeleteAsync(uri);
                Assert.IsTrue(response.IsSuccessStatusCode);

                issue = await scope.Helper.GetIssueAsync(scope.Helper.Issue.Id);
                var exits = issue.IssueDetail.Requirements?.FirstOrDefault(x => x.Requirement.Equals("Die Application Testen")) != null;
                Assert.IsFalse(exits);
            }
        }
    }
}