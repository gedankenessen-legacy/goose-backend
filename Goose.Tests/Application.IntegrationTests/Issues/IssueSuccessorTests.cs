using System;
using System.Collections.Generic;
using System.Net;
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
            using var helper = await new SimpleTestHelperBuilder().Build();
            var tempPredecessor = helper.Issue.Copy();
            tempPredecessor.Id = ObjectId.Empty;
            tempPredecessor.IssueDetail.StartDate = DateTime.Now.AddHours(3);
            var predecessor = await helper.CreateIssue(tempPredecessor).Parse<IssueDTO>();
            var res = await helper.SetPredecessor(predecessor.Id);
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
        }
    }
}