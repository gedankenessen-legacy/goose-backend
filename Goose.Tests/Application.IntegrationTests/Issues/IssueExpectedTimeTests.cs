using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Identity;
using MongoDB.Bson;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    class IssueExpectedTimeBuilder : SimpleTestHelperBuilder
    {
        public override Task<IssueDTO> CreateIssue(HttpClient client, SimpleTestHelper helper)
        {
            _issueDto.IssueDetail.RequirementsNeeded = false;
            return base.CreateIssue(client, helper);
        }
    }

    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class IssueExpectedTimeTests
    {
        [Test]
        public async Task CannotEditExpectedTimeWhenReqNeeded()
        {
            var helper = await new SimpleTestHelperBuilder().Build();
            helper.Issue.IssueDetail.ExpectedTime = 222;
            var res = await helper.Helper.UpdateIssue(helper.Issue);
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Test]
        public async Task EditExpectedTimeAsLeader()
        {
            var helper = await new IssueExpectedTimeBuilder().Build();
            helper.Issue.IssueDetail.ExpectedTime = 222;
            var res = await helper.Helper.UpdateIssue(helper.Issue);
            Assert.AreEqual(HttpStatusCode.NoContent, res.StatusCode);
            var newIssue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual(helper.Issue.IssueDetail.ExpectedTime, newIssue.IssueDetail.ExpectedTime);
        }

        [Test]
        public async Task EditExpectedTimeAsEmployee()
        {
            var helper = await new IssueExpectedTimeBuilder().Build();
            helper.Issue.IssueDetail.ExpectedTime = 222;
            await helper.GenerateUserAndSetToProject(Role.EmployeeRole);

            var res = await helper.Helper.UpdateIssue(helper.Issue);
            Assert.AreEqual(HttpStatusCode.NoContent, res.StatusCode);
            var newIssue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual(helper.Issue.IssueDetail.ExpectedTime, newIssue.IssueDetail.ExpectedTime);
        }


        [Test]
        public async Task CannotEditExpectedTimeAsCustomer()
        {
            var helper = await new IssueExpectedTimeBuilder().Build();
            helper.Issue.IssueDetail.ExpectedTime = 222;
            await helper.GenerateUserAndSetToProject(Role.CustomerRole);

            var res = await helper.Helper.UpdateIssue(helper.Issue);
            Assert.AreEqual(HttpStatusCode.Forbidden, res.StatusCode);
        }

        [Test]
        public async Task CannotEditExpectedTimeAsReadonlyEmployee()
        {
            var helper = await new IssueExpectedTimeBuilder().Build();
            helper.Issue.IssueDetail.ExpectedTime = 222;
            await helper.GenerateUserAndSetToProject(Role.ReadonlyEmployeeRole);

            var res = await helper.Helper.UpdateIssue(helper.Issue);
            Assert.AreEqual(HttpStatusCode.Forbidden, res.StatusCode);
        }
        [Test]
        public async Task CanEditExpectedTimeWithChild()
        {
            var helper = await new IssueExpectedTimeBuilder().Build();
            await helper.CreateChild();
            helper.Issue.IssueDetail.ExpectedTime = 222;

            var res = await helper.Helper.UpdateIssue(helper.Issue);
            Assert.AreEqual(HttpStatusCode.NoContent, res.StatusCode);
            var newIssue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual(helper.Issue.IssueDetail.ExpectedTime, newIssue.IssueDetail.ExpectedTime);
        }

        [Test]
        public async Task CannotEditExpectedTimeBecauseParentHasLess()
        {
            var helper = await new IssueExpectedTimeBuilder().Build();
            var child = await helper.CreateChild();
            
            child.IssueDetail.ExpectedTime = 222;

            var res = await helper.Helper.UpdateIssue(child);
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Test]
        public async Task CannotAddChildBecauseExpectedTimeOfParentIsExceeded()
        {
            var helper = await new IssueExpectedTimeBuilder().Build();
            var copy = helper.Issue.Copy();
            copy.Id = ObjectId.Empty;
            copy.IssueDetail.ExpectedTime++;
            var child = await helper.CreateIssue(copy).Parse<IssueDTO>();
            var res = await helper.SetIssueChild(child.Id);
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
        }
    }
}