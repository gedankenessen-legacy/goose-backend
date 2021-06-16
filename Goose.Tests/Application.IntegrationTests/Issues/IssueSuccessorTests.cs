using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Goose.API.Utils;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Projects;
using MongoDB.Bson;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class IssueSuccessorTests
    {
        private class SimpleTestHelperBuilderSuccessor : SimpleTestHelperBuilder
        {
            public override async Task<SimpleTestHelper> Build()
            {
                var helper = await base.Build();
                var issue = base.GetIssueDTOCopy(helper.client, helper);
                issue.IssueDetail.Visibility = true;
                issue.IssueDetail.EndDate = DateTime.Now.AddHours(2);
                var responce = await helper.CreateIssue(issue);
                helper.Issue = await responce.Parse<IssueDTO>();
                return helper;
            }

            public override Task<IssueDTO> CreateIssue(HttpClient client, SimpleTestHelper helper) => null;

        }

        [Test]
        public async Task CanAddSuccessor()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var predecessor = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.SetPredecessor(predecessor.Id);

            var predecessors = helper.Helper.GetPredecessors(helper.Issue.Id);
            var successors = helper.Helper.GetSuccessors(predecessor.Id);

            Assert.IsTrue((await predecessors.Parse<List<IssueDTO>>()).HasWhere(it => it.Id == predecessor.Id));
            Assert.IsTrue((await successors.Parse<List<IssueDTO>>()).HasWhere(it => it.Id == helper.Issue.Id));
        }

        [Test]
        public async Task IssueHasToWaitUntilPredecessorFinishes()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var predecessor = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.SetPredecessor(predecessor.Id);


            await helper.SetState(State.NegotiationState);
            await helper.SetState(State.ProcessingState);
            var newIssue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual(State.BlockedState, (await helper.Helper.GetStateById(newIssue.ProjectId, newIssue.StateId)).Name);

            await helper.Helper.SetStateOfIssue(predecessor, State.CancelledState);
            newIssue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual(State.ProcessingState, (await helper.Helper.GetStateById(newIssue.ProjectId, newIssue.StateId)).Name);
        }

        [Test]
        public async Task CannotSetParentAsPredecessor()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var parent = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.Helper.SetParentIssue(parent.Id, helper.Issue.Id);
            var res = await helper.SetPredecessor(parent.Id);
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);

            var child = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.IsTrue(child.PredecessorIssueIds.Count == 0);
        }

        [Test]
        public async Task CannotSetChildAsPredecessor()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var child = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.SetIssueChild(child.Id);
            var res = await helper.Helper.SetPredecessor(helper.Issue.Id, child.Id);
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Test]
        public async Task CannotAddPredecessorInConclusionPhase()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var predecessor = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.Helper.SetStateOfIssue(predecessor, State.NegotiationState);
            await helper.Helper.SetStateOfIssue(predecessor, State.ProcessingState);
            await helper.Helper.SetStateOfIssue(predecessor, State.ReviewState);
            await helper.Helper.SetStateOfIssue(predecessor, State.CompletedState);

            var res = await helper.SetPredecessor(predecessor.Id);
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Test]
        public async Task StartDateOfPredecessorCannotBeAfterEndDateOfSuccessor()
        {
            using var helper = await new SimpleTestHelperBuilderSuccessor().Build();
            var tempPredecessor = helper.Issue.Copy();
            tempPredecessor.Id = ObjectId.Empty;
            tempPredecessor.IssueDetail.StartDate = DateTime.Now.AddHours(3);
            var predecessor = await helper.CreateIssue(tempPredecessor).Parse<IssueDTO>();
            var res = await helper.SetPredecessor(predecessor.Id);
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Test]
        public async Task StartDateOfPredecessorCannotBeAfterEndDateOfSuccessorThrewUpdate()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var tempPredecessor = helper.Issue.Copy();
            tempPredecessor.Id = ObjectId.Empty;
            var predecessor = await helper.CreateIssue(tempPredecessor).Parse<IssueDTO>();
            var res = await helper.SetPredecessor(predecessor.Id);

            //Issue Nachfolger
            //predeccessor Vorgänger
            var issue = await helper.Helper.GetIssueThroughClientAsync(helper.Issue.Project.Id, helper.Issue.Id);
            issue.IssueDetail.EndDate = DateTime.Now.AddHours(2);

            var uri = $"api/projects/{issue.Project.Id}/issues/{issue.Id}";

            var response = await helper.client.PutAsync(uri, issue.ToStringContent());

            Assert.IsTrue(response.IsSuccessStatusCode);

            var predecessorToUpdate = await helper.Helper.GetIssueThroughClientAsync(predecessor.Project.Id, predecessor.Id);

            predecessorToUpdate.IssueDetail.StartDate = DateTime.Now.AddHours(3);

            uri = $"api/projects/{predecessorToUpdate.Project.Id}/issues/{predecessorToUpdate.Id}";

            response = await helper.client.PutAsync(uri, predecessorToUpdate.ToStringContent());

            Assert.IsFalse(response.IsSuccessStatusCode);
        }

        [Test]
        public async Task EnddateOfSuccessorCannotBeBeforStartDateOfPredeccessorThrewUpdate()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var tempPredecessor = helper.Issue.Copy();
            tempPredecessor.Id = ObjectId.Empty;
            var predecessor = await helper.CreateIssue(tempPredecessor).Parse<IssueDTO>();
            var res = await helper.SetPredecessor(predecessor.Id);

            //Issue Nachfolger
            //predeccessor Vorgänger

            var predecessorToUpdate = await helper.Helper.GetIssueThroughClientAsync(predecessor.Project.Id, predecessor.Id);

            predecessorToUpdate.IssueDetail.StartDate = DateTime.Now.AddHours(3);

            var uri = $"api/projects/{predecessorToUpdate.Project.Id}/issues/{predecessorToUpdate.Id}";

            var response = await helper.client.PutAsync(uri, predecessorToUpdate.ToStringContent());

            Assert.IsTrue(response.IsSuccessStatusCode);

            var issue = await helper.Helper.GetIssueThroughClientAsync(helper.Issue.Project.Id, helper.Issue.Id);
            issue.IssueDetail.EndDate = DateTime.Now.AddHours(2);

            uri = $"api/projects/{issue.Project.Id}/issues/{issue.Id}";

            response = await helper.client.PutAsync(uri, issue.ToStringContent());

            Assert.IsFalse(response.IsSuccessStatusCode);
        }

        [Test]
        public async Task EndDateOfPredecessorCanBeAfterStartDateOfSuccessorThrewUpdate()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var tempPredecessor = helper.Issue.Copy();
            tempPredecessor.Id = ObjectId.Empty;
            var predecessor = await helper.CreateIssue(tempPredecessor).Parse<IssueDTO>();
            var res = await helper.SetPredecessor(predecessor.Id);

            //Issue Nachfolger
            //predeccessor Vorgänger
            var issue = await helper.Helper.GetIssueThroughClientAsync(helper.Issue.Project.Id, helper.Issue.Id);
            issue.IssueDetail.EndDate = DateTime.Now.AddHours(2);

            var uri = $"api/projects/{issue.Project.Id}/issues/{issue.Id}";

            var response = await helper.client.PutAsync(uri, issue.ToStringContent());

            Assert.IsTrue(response.IsSuccessStatusCode);

            var predecessorToUpdate = await helper.Helper.GetIssueThroughClientAsync(predecessor.Project.Id, predecessor.Id);

            predecessorToUpdate.IssueDetail.StartDate = DateTime.Now.AddHours(1);

            uri = $"api/projects/{predecessorToUpdate.Project.Id}/issues/{predecessorToUpdate.Id}";

            response = await helper.client.PutAsync(uri, predecessorToUpdate.ToStringContent());

            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [Test]
        public async Task CannotAddSamePredecessorMultipleTimes()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var predecessor = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.SetPredecessor(predecessor.Id);
            var res = await helper.SetPredecessor(predecessor.Id);
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
        }
    }
}