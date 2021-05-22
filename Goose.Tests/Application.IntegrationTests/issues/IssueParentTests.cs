using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Identity;
using MongoDB.Bson;
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

        [Test]
        public async Task CannotSetIssueAsOwnParent()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            //Set Parent
            var setParentRes = await helper.AddIssueChild(helper.Issue.Id);
            Assert.AreEqual(HttpStatusCode.BadRequest, setParentRes.StatusCode);
        }

        [Test]
        public async Task CannotSetChildAsParent()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var child = await helper.CreateIssue().Parse<IssueDTO>();
            //Set Parent
            await helper.AddIssueChild(child.Id);
            var setParentRes = await helper.Helper.SetParentIssue(child.Id, helper.Issue.Id);
            Assert.AreEqual(HttpStatusCode.BadRequest, setParentRes.StatusCode);
        }

        [Test]
        public async Task CannotSetParentOfIssueInTree()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var child1 = await helper.CreateIssue().Parse<IssueDTO>();
            var child2 = await helper.CreateIssue().Parse<IssueDTO>();
            //Set Parent
            await helper.AddIssueChild(child1.Id);
            await helper.AddIssueChild(child2.Id);
            var setParentRes = await helper.Helper.SetParentIssue(child1.Id, child2.Id);
            Assert.AreEqual(HttpStatusCode.BadRequest, setParentRes.StatusCode);
        }

        [Test]
        public async Task CannotSetParentFromAnotherProject()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var project2 = await helper.CreateProjectAndAddProjectUser(Role.ProjectLeaderRole).Parse<ProjectDTO>();
            
            var issueCopy = helper.Issue.Copy();
            issueCopy.Id = ObjectId.Empty;
            issueCopy.Project = project2;
            
            var issue2 = await helper.Helper.CreateIssue(project2.Id, issueCopy).Parse<IssueDTO>();
            var res = await helper.Helper.SetParentIssue(helper.Issue.Id, issue2.Id);
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Test]
        public async Task DependentPropertiesGetSetWhenParentIsAdded()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var parentIssue = helper.Issue;

            var childIssueDto = parentIssue.Copy();
            childIssueDto.Id = ObjectId.Empty;

            // TODO check predecessors
            childIssueDto.IssueDetail.Priority = parentIssue.IssueDetail.Priority + 1;

            childIssueDto = await helper.CreateIssue(childIssueDto).Parse<IssueDTO>();
            await helper.Helper.SetParentIssue(parentIssue.Id, childIssueDto.Id);

            var childIssue = await helper.Helper.GetIssueAsync(childIssueDto.Id);

            Assert.AreEqual(parentIssue.IssueDetail.Priority, childIssue.IssueDetail.Priority);
        }

        [Test]
        public async Task VisibilityStatusOfParentAndChildMustBeIndentical()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var parentIssue = helper.Issue;

            var childIssue = parentIssue.Copy();
            childIssue.Id = ObjectId.Empty;

            // TODO check predecessors
            childIssue.IssueDetail.Visibility = !parentIssue.IssueDetail.Visibility;

            childIssue = await helper.CreateIssue(childIssue).Parse<IssueDTO>();
            var result = await helper.Helper.SetParentIssue(parentIssue.Id, childIssue.Id);

            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task DependentPropertiesGetPropagatedDown()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var parentIssueDto = helper.Issue;

            var childIssueDto = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.Helper.SetParentIssue(parentIssueDto.Id, childIssueDto.Id);

            var grandChildIssueDto = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.Helper.SetParentIssue(childIssueDto.Id, grandChildIssueDto.Id);

            parentIssueDto.IssueDetail.Priority += 1;

            var result = await helper.Helper.PutIssue(parentIssueDto);
            Assert.IsTrue(result.IsSuccessStatusCode);

            var childIssue = await helper.GetIssueAsync(childIssueDto.Id);
            var grandChildIssue = await helper.GetIssueAsync(grandChildIssueDto.Id);

            Assert.AreEqual(parentIssueDto.IssueDetail.Priority, childIssue.IssueDetail.Priority);
            Assert.AreEqual(parentIssueDto.IssueDetail.Priority, grandChildIssue.IssueDetail.Priority);
        }

        [Test]
        public async Task DependentPropertiesOnChildrenCannotBeSet()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var parentIssueDto = helper.Issue;

            var childIssueDto = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.Helper.SetParentIssue(parentIssueDto.Id, childIssueDto.Id);

            childIssueDto.IssueDetail.Priority = parentIssueDto.IssueDetail.Priority + 1;

            var result = await helper.Helper.PutIssue(parentIssueDto);
            Assert.IsTrue(result.IsSuccessStatusCode);

            var childIssue = await helper.GetIssueAsync(childIssueDto.Id);

            Assert.AreEqual(parentIssueDto.IssueDetail.Priority, childIssue.IssueDetail.Priority);

        }
    }
}