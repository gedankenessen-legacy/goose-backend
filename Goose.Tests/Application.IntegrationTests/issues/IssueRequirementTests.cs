using System.Linq;
using System.Threading.Tasks;
using Goose.Domain.Models.Issues;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    class IssueRequirementTests
    {
        [Test]
        public async Task AddRequirement()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
                IssueRequirement issueRequirement = new IssueRequirement() {Requirement = "Die Application Testen"};
                var uri = $"/api/issues/{helper.Issue.Id}/requirements/";
                var response = await helper.client.PostAsync(uri, issueRequirement.ToStringContent());
                Assert.IsTrue(response.IsSuccessStatusCode);

                var issue = await helper.GetIssueAsync(helper.Issue.Id);
                var exits = issue.IssueDetail.Requirements?.FirstOrDefault(x => x.Requirement.Equals("Die Application Testen")) != null;
                Assert.IsTrue(exits);
        }

        [Test]
        public async Task AddRequirementFalse()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
                IssueRequirement issueRequirement = new IssueRequirement() {Requirement = " "};
                var uri = $"/api/issues/{helper.Issue.Id}/requirements/";
                var responce = await helper.client.PostAsync(uri, issueRequirement.ToStringContent());
                Assert.IsFalse(responce.IsSuccessStatusCode);

                var issue = await helper.GetIssueAsync(helper.Issue.Id);
                var exits = issue.IssueDetail.Requirements?.FirstOrDefault(x => x.Requirement.Equals("Die Application Testen")) != null;
                Assert.IsFalse(exits);
        }

        [Test]
        public async Task DeleteRequirement()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
                IssueRequirement issueRequirement = new IssueRequirement() {Requirement = "Die Application Testen"};
                var uri = $"/api/issues/{helper.Issue.Id}/requirements/";
                var response = await helper.client.PostAsync(uri, issueRequirement.ToStringContent());
                Assert.IsTrue(response.IsSuccessStatusCode);

                var issue = await helper.GetIssueAsync(helper.Issue.Id);
                var addedRequirement = issue.IssueDetail.Requirements?.FirstOrDefault(x => x.Requirement.Equals("Die Application Testen"));
                uri = $"/api/issues/{issue.Id}/requirements/{addedRequirement.Id}";
                response = await helper.client.DeleteAsync(uri);
                Assert.IsTrue(response.IsSuccessStatusCode);

                issue = await helper.GetIssueAsync(helper.Issue.Id);
                var exits = issue.IssueDetail.Requirements?.FirstOrDefault(x => x.Requirement.Equals("Die Application Testen")) != null;
                Assert.IsFalse(exits);
        }
    }
}