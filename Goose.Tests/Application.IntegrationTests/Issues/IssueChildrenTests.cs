using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Goose.API.Utils;
using Goose.Domain.DTOs.Issues;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class IssueChildrenTests
    {
        [Test]
        public async Task GetChildren()
        {
            var helper = await new SimpleTestHelperBuilder().Build();
            var child = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.SetIssueChild(child.Id);

            var res = await helper.Helper.GetChildrenIssues(helper.Issue.Id);
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            var children = await res.Parse<List<IssueDTO>>();
            Assert.AreEqual(1, children.Count);
            Assert.AreEqual(child.Id, children[0].Id);
        }
        [Test]
        public async Task GetChildrenRecursive()
        {
            var helper = await new SimpleTestHelperBuilder().Build();
            var child = await helper.CreateIssue().Parse<IssueDTO>();
            var childOfChild = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.SetIssueChild(child.Id);
            await helper.Helper.SetParentIssue(child.Id, childOfChild.Id);

            var res = await helper.Helper.GetChildrenIssues(helper.Issue.Id, true);
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            var children = await res.Parse<List<IssueDTO>>();
            Assert.AreEqual(2, children.Count);
            Assert.IsTrue(children.HasWhere(it => it.Id == child.Id));
            Assert.IsTrue(children.HasWhere(it => it.Id == childOfChild.Id));
        }
    }
}