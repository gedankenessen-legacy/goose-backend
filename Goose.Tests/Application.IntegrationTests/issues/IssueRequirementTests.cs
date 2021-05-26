using Goose.Domain.Models.Auth;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Issues;
using Goose.Domain.Models.Projects;
using MongoDB.Bson;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.Tests.Application.IntegrationTests.Issues
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    class IssueRequirementTests
    {
        [Test]
        public async Task AddRequirement()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var res = await helper.Helper.GenerateRequirement();
            Assert.NotNull(res);

            var issue = await helper.GetIssueAsync(helper.Issue.Id);
            var exits = issue.IssueDetail.Requirements?.FirstOrDefault(x => x.Requirement.Equals("Die Application Testen")) != null;
            Assert.IsTrue(exits);
        }

        [Test]
        public async Task DeleteRequirement()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
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

        [Test]
        public async Task MarkRequirementAsDone()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();

            // create requirement
            var res = await helper.Helper.GenerateRequirement();
            Assert.NotNull(res);

            // mark as done
            res.Achieved = true;
            var uri = $"/api/issues/{helper.Issue.Id}/requirements/{res.Id}";
            var response = await helper.client.PutAsync(uri, res.ToStringContent());

            var issue = await helper.GetIssueAsync(helper.Issue.Id);
            var achieved = issue.IssueDetail.Requirements?.FirstOrDefault(x => x.Achieved == true) != null;
            Assert.IsTrue(achieved);
        }

        [Test]
        public async Task AddRequirementAsCustomerFalse()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();

            var customerUser = await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.CustomerRole);

            var customerSignIn = await helper.Helper.SignIn(new SignInRequest()
            {
                Username = customerUser.Username,
                Password = helper.Helper.UsedPasswordForTests
            });

            helper.Helper.SetAuth(customerSignIn);

            var res = await helper.Helper.GenerateRequirement();
            Assert.True(ObjectId.Empty.Equals(res.Id));
        }
    
        [Test]
        public async Task AddRequirementInProcessingPhaseFalse()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();

            var issue = helper.Issue.Copy();
            issue.State = await helper.Helper.GetStateByNameAsync(helper.Project.Id, State.ProcessingState);
            var uri = $"api/projects/{issue.Project.Id}/issues/{issue.Id}";
            var response = await helper.client.PutAsync(uri, issue.ToStringContent());

            var res = await helper.Helper.GenerateRequirement();
            Assert.True(ObjectId.Empty.Equals(res.Id));
        }
    }
}