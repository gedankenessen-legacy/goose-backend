using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Goose.Domain.DTOs.Issues;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class IssueParentTests
    {
        [Test]
        public async Task IssueCanHaveParent()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var child = await helper.CreateIssue().Parse<IssueDTO>();

            //Set Parent
            var setParentRes = await helper.AddIssueChild(child.Id);
            Assert.AreEqual(HttpStatusCode.NoContent, setParentRes.StatusCode);

            //Check if child issue has parent field set
            var getParentRes = await helper.Helper.GetParentIssue(child.Id);
            Assert.AreEqual(HttpStatusCode.OK, getParentRes.StatusCode);
            Assert.AreEqual(helper.Issue.Id, (await getParentRes.Parse<IssueDTO>()).Id);

            //Check if parent Issue has child
            var getChildrenRes = await helper.Helper.GetChildrenIssues(helper.Issue.Id);
            Assert.AreEqual(HttpStatusCode.OK, getChildrenRes.StatusCode);
            Assert.IsTrue((await getChildrenRes.Parse<List<IssueDTO>>()).FirstOrDefault(it => it.Id.Equals(child.Id)) != null);
        }
    }
}