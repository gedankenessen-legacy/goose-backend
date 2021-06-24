using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Projects;
using MongoDB.Bson;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.Issues
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
            var setParentRes = await helper.SetIssueChild(child.Id);
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
            var setParentRes = await helper.SetIssueChild(helper.Issue.Id);
            Assert.AreEqual(HttpStatusCode.BadRequest, setParentRes.StatusCode);
        }

        [Test]
        public async Task CannotSetChildAsParent()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var child = await helper.CreateIssue().Parse<IssueDTO>();
            //Set Parent
            await helper.SetIssueChild(child.Id);
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
            await helper.SetIssueChild(child1.Id);
            await helper.SetIssueChild(child2.Id);
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
            var parentIssueDto = helper.Issue;

            var predecessorDto = await helper.CreateIssue().Parse<IssueDTO>();
            var uri = $"api/issues/{parentIssueDto.Id}/predecessors/{predecessorDto.Id}";
            var result = await helper.client.PutAsync(uri, null);
            Assert.IsTrue(result.IsSuccessStatusCode);

            var childIssueDto = parentIssueDto.Copy();
            childIssueDto.Id = ObjectId.Empty;

            childIssueDto.IssueDetail.Priority = parentIssueDto.IssueDetail.Priority + 1;

            childIssueDto = await helper.CreateIssue(childIssueDto).Parse<IssueDTO>();
            await helper.Helper.SetParentIssue(parentIssueDto.Id, childIssueDto.Id);

            var childIssue = await helper.Helper.GetIssueAsync(childIssueDto.Id);

            Assert.AreEqual(parentIssueDto.IssueDetail.Priority, childIssue.IssueDetail.Priority);
            Assert.AreEqual(1, childIssue.InheritedPredecessorIssueIds.Count);
            Assert.AreEqual(predecessorDto.Id, childIssue.InheritedPredecessorIssueIds[0]);
        }

        [Test]
        public async Task VisibilityStatusOfParentAndChildMustBeIndentical()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var parentIssueDto = helper.Issue;

            var childIssueDto = parentIssueDto.Copy();
            childIssueDto.Id = ObjectId.Empty;

            childIssueDto.IssueDetail.Visibility = !parentIssueDto.IssueDetail.Visibility;

            childIssueDto = await helper.CreateIssue(childIssueDto).Parse<IssueDTO>();
            var result = await helper.Helper.SetParentIssue(parentIssueDto.Id, childIssueDto.Id);

            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task DependentPropertiesGetPropagatedDown()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var parentIssueDto = helper.Issue;

            var predecessorDto = await helper.CreateIssue().Parse<IssueDTO>();
            var uri = $"api/issues/{parentIssueDto.Id}/predecessors/{predecessorDto.Id}";
            var result = await helper.client.PutAsync(uri, null);
            Assert.IsTrue(result.IsSuccessStatusCode);

            var childIssueDto = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.Helper.SetParentIssue(parentIssueDto.Id, childIssueDto.Id);

            var grandChildIssueDto = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.Helper.SetParentIssue(childIssueDto.Id, grandChildIssueDto.Id);

            parentIssueDto.IssueDetail.Priority += 1;

            result = await helper.Helper.UpdateIssue(parentIssueDto);
            Assert.IsTrue(result.IsSuccessStatusCode);

            var childIssue = await helper.GetIssueAsync(childIssueDto.Id);
            var grandChildIssue = await helper.GetIssueAsync(grandChildIssueDto.Id);

            Assert.AreEqual(parentIssueDto.IssueDetail.Priority, childIssue.IssueDetail.Priority);
            Assert.AreEqual(1, childIssue.InheritedPredecessorIssueIds.Count);
            Assert.AreEqual(predecessorDto.Id, childIssue.InheritedPredecessorIssueIds[0]);

            Assert.AreEqual(parentIssueDto.IssueDetail.Priority, grandChildIssue.IssueDetail.Priority);
            Assert.AreEqual(1, grandChildIssue.InheritedPredecessorIssueIds.Count);
            Assert.AreEqual(predecessorDto.Id, grandChildIssue.InheritedPredecessorIssueIds[0]);
        }

        [Test]
        public async Task DependentPropertiesOnChildrenCannotBeSet()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var parentIssueDto = helper.Issue;

            var childIssueDto = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.Helper.SetParentIssue(parentIssueDto.Id, childIssueDto.Id);

            childIssueDto.IssueDetail.Priority = parentIssueDto.IssueDetail.Priority + 1;

            var result = await helper.Helper.UpdateIssue(parentIssueDto);
            Assert.IsTrue(result.IsSuccessStatusCode);

            var childIssue = await helper.GetIssueAsync(childIssueDto.Id);

            Assert.AreEqual(parentIssueDto.IssueDetail.Priority, childIssue.IssueDetail.Priority);

        }

        [Test]
        public async Task CannotAddChildAfterReviewState()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var child = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.AcceptSummary();
            await helper.SetState(State.ReviewState);

            var resReviewState = await helper.SetIssueChild(child.Id);
            Assert.AreEqual(HttpStatusCode.BadRequest, resReviewState.StatusCode);
            
            await helper.SetState(State.CompletedState);
            var resCompletedState = await helper.SetIssueChild(child.Id);
            Assert.AreEqual(HttpStatusCode.BadRequest, resCompletedState.StatusCode);
        }
    }
}