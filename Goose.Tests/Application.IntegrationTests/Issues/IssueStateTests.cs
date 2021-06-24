using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Projects;
using MongoDB.Bson;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class IssueStateTests
    {
        [Test]
        public async Task CorrectStateOrderWorks()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var issue = helper.Issue;
            var states = await helper.Helper.GetStateListAsync(helper.Issue.Project.Id);

            var updateStates = new List<StateDTO>();
            updateStates.Add(states.First(it => it.Name == State.NegotiationState));
            updateStates.Add(states.First(it => it.Name == State.ProcessingState));
            updateStates.Add(states.First(it => it.Name == State.ReviewState));
            updateStates.Add(states.First(it => it.Name == State.CompletedState));
            updateStates.Add(states.First(it => it.Name == State.ArchivedState));

            foreach (var state in updateStates)
            {
                HttpResponseMessage res;
                if (state.Name == State.ProcessingState)
                {
                    await helper.AcceptSummary();
                }
                else
                {
                    issue.State = state;
                    res = await helper.Helper.UpdateIssue(issue);
                    Assert.AreEqual(HttpStatusCode.NoContent, res.StatusCode);
                }
            }
        }

        /*
         * Fügt n Issues hinzu (anhängig der Anzahl an Statusse die in updateStates sind).
         * In jeden Schleifendurchlauf wird ein Issue gecancelt, die anderen Issue werden in den nächsten Status gesetzt, die getestet werden sollen
         */
        [Test]
        public async Task CancelIssues()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var states = await helper.Helper.GetStateListAsync(helper.Issue.Project.Id);
            var updateStates = new List<StateDTO>();
            var cancelState = states.First(it => it.Name == State.CancelledState);
            updateStates.Add(states.First(it => it.Name == State.CheckingState));
            updateStates.Add(states.First(it => it.Name == State.NegotiationState));
            updateStates.Add(states.First(it => it.Name == State.ProcessingState));
            updateStates.Add(states.First(it => it.Name == State.ReviewState));

            var issues = await Task.WhenAll((await Task.WhenAll(updateStates.Select(it => helper.CreateIssue()))).Select(it => it.Parse<IssueDTO>()).ToList());
            for (int i = 0; i < updateStates.Count; i++)
            {
                var state = updateStates[i];
                issues[i].State = cancelState;
                for (int j = i; j < updateStates.Count; j++) issues[j].State = state;

                await Task.WhenAll(issues.Skip(i).ToList().Select(it => helper.Helper.UpdateIssue(it)));
            }
        }

        [Test]
        public async Task ChildGetsCancelledOnParentCancel()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var child = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.SetIssueChild(child.Id);

            var cancel = await helper.SetState(State.CancelledState);
            Assert.AreEqual(HttpStatusCode.NoContent, cancel.StatusCode);
            var newChild = await helper.GetIssueAsync(child.Id);
            Assert.AreEqual((await helper.Helper.GetStateByNameAsync(helper.Project.Id, State.CancelledState)).Id, newChild.StateId);
        }

        [Test]
        public async Task SuccessorMovesOutOfBlockedOnPredecessorCancel()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var predecessor1 = await helper.CreateIssue().Parse<IssueDTO>();
            var predecessor2 = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.SetPredecessor(predecessor1.Id);
            await helper.SetPredecessor(predecessor2.Id);

            await helper.AcceptSummary();

            var states = helper.Helper.GetStateListAsync(helper.Project.Id);
            var newIssue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual((await states).First(it => it.Name == State.BlockedState).Id, newIssue.StateId);

            await helper.Helper.SetStateOfIssue(predecessor1, State.CancelledState);
            newIssue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual((await states).First(it => it.Name == State.BlockedState).Id, newIssue.StateId);

            await helper.Helper.SetStateOfIssue(predecessor2, State.CancelledState);
            newIssue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual((await states).First(it => it.Name == State.ProcessingState).Id, newIssue.StateId);
        }

        [Test]
        public async Task IssueIsWaitingIfStartDateNotReached()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var copy = helper.Issue.Copy();
            copy.Id = ObjectId.Empty;
            copy.IssueDetail.StartDate = DateTime.Now.AddHours(3);
            var res = await helper.CreateIssue(copy);
            Assert.AreEqual(HttpStatusCode.Created, res.StatusCode);
            var issue = await res.Parse<IssueDTO>();
            await helper.Helper.AcceptSummary(issue.Id);

            var newCopy = await helper.GetIssueAsync(issue.Id);
            Assert.AreEqual(State.WaitingState, (await helper.Helper.GetStateById(newCopy)).Name);
        }

        [Test]
        public async Task FromCheckingToUserGeneratedStateInNegotiationPhase()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var state = await helper.CreateState(new StateDTO
            {
                Name = $"{State.NegotiationState}22",
                Phase = State.NegotiationPhase,
                UserGenerated = true
            }).Parse<StateDTO>();
            var res = await helper.SetState(state.Name);
            Assert.AreEqual(HttpStatusCode.NoContent, res.StatusCode);

            var issue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual((await helper.Helper.GetStateById(issue.ProjectId, issue.StateId)).Name, state.Name);
        }

        [Test]
        public async Task FromNegotiationToUserGeneratedStateInNegotiationPhaseAndBack()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var state = await helper.CreateState(new StateDTO
            {
                Name = $"{State.NegotiationState}22",
                Phase = State.NegotiationPhase,
                UserGenerated = true
            }).Parse<StateDTO>();
            await helper.SetState(state.Name);
            var res = await helper.SetState((await helper.Helper.GetStateByNameAsync(helper.Project.Id, State.NegotiationState)).Name);
            Assert.AreEqual(HttpStatusCode.NoContent, res.StatusCode);

            var issue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual(State.NegotiationState, (await helper.Helper.GetStateById(issue.ProjectId, issue.StateId)).Name);
        }

        [Test]
        public async Task FromProcessingStateToUserGeneratedStateInProcessingPhasePhaseAndBack()

        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            await helper.AcceptSummary();
            var state = await helper.CreateState(new StateDTO
            {
                Name = $"{State.ProcessingState}22",
                Phase = State.ProcessingPhase,
                UserGenerated = true
            }).Parse<StateDTO>();
            await helper.SetState(state.Name);
            var res = await helper.SetState((await helper.Helper.GetStateByNameAsync(helper.Project.Id, State.ProcessingState)).Name);
            Assert.AreEqual(HttpStatusCode.NoContent, res.StatusCode);

            var issue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual(State.ProcessingState, (await helper.Helper.GetStateById(issue.ProjectId, issue.StateId)).Name);
        }

        [Test]
        public async Task FromCustomPrecessingStateToReview()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            await helper.AcceptSummary();
            var state = await helper.CreateState(new StateDTO
            {
                Name = $"{State.ProcessingState}22",
                Phase = State.ProcessingPhase,
                UserGenerated = true
            }).Parse<StateDTO>();
            await helper.SetState(state.Name);
            var res = await helper.SetState(State.ReviewState);
            Assert.AreEqual(HttpStatusCode.NoContent, res.StatusCode);

            var issue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual(State.ReviewState, (await helper.Helper.GetStateById(issue.ProjectId, issue.StateId)).Name);
        }

        [Test]
        public async Task FromCompletionStateToUserGeneratedStateInConclusionPhasePhaseAndBack()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            await helper.AcceptSummary();
            await helper.SetState(State.ReviewState);
            await helper.SetState(State.CompletedState);
            var state = await helper.CreateState(new StateDTO
            {
                Name = $"{State.CompletedState}22",
                Phase = State.ConclusionPhase,
                UserGenerated = true
            }).Parse<StateDTO>();
            await helper.SetState(state.Name);
            var res = await helper.SetState((await helper.Helper.GetStateByNameAsync(helper.Project.Id, State.CompletedState)).Name);
            Assert.AreEqual(HttpStatusCode.NoContent, res.StatusCode);

            var issue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual(State.CompletedState, (await helper.Helper.GetStateById(issue.ProjectId, issue.StateId)).Name);
        }

        [Test]
        public async Task ParentIsBlockedIfPredecessorAddedInPrecessingPhase()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var predecessor = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.AcceptSummary();
            var issue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual(State.ProcessingState, (await helper.Helper.GetStateById(issue.ProjectId, issue.StateId)).Name);
            await helper.SetPredecessor(predecessor.Id);
            issue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual(State.BlockedState, (await helper.Helper.GetStateById(issue.ProjectId, issue.StateId)).Name);
        }

        [Test]
        public async Task CanChangeStartDateInProcessingPhaseIfRequirementsSkipped()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var copy = helper.Issue.Copy();
            copy.IssueDetail.RequirementsNeeded = false;
            copy.Id = ObjectId.Empty;
            var issue = await helper.CreateIssue(copy).Parse<IssueDTO>();
            Assert.AreEqual(State.ProcessingState, (await helper.Helper.GetStateById(issue)).Name);
            var oldTime = issue.IssueDetail.StartDate;
            issue.IssueDetail.StartDate = DateTime.Now;
            await helper.Helper.UpdateIssue(issue);
            var updatedIssue = await helper.GetIssueAsync(issue.Id);
            Assert.AreNotEqual(oldTime, updatedIssue.IssueDetail.StartDate);
        }

        [Test]
        public async Task CannotChangeStartDateInProcessingPhaseIfRequirementsSkipped()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            await helper.AcceptSummary();
            Assert.AreEqual(State.ProcessingState, (await helper.Helper.GetStateById(await helper.GetIssueAsync(helper.Issue.Id))).Name);
            var oldTime = helper.Issue.IssueDetail.StartDate;
            helper.Issue.IssueDetail.StartDate = DateTime.Now;
            await helper.Helper.UpdateIssue(helper.Issue);
            var updatedIssue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual(oldTime, updatedIssue.IssueDetail.StartDate);
        }

        [Test]
        public async Task ParentNotBlockedIfChildAddedInProcessingPhase()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            await helper.AcceptSummary();
            var child = await helper.CreateChild();
            Assert.AreEqual(State.ProcessingState, (await helper.Helper.GetStateById(await helper.GetIssueAsync(helper.Issue.Id))).Name);
            await helper.SetState(State.ReviewState);
            Assert.AreEqual(HttpStatusCode.BadRequest, (await helper.SetState(State.ReviewState)).StatusCode);

            await helper.Helper.SetStateOfIssue(child, State.CancelledState);
            await helper.SetState(State.ReviewState);
            Assert.AreEqual(State.ReviewState, (await helper.Helper.GetStateById(await helper.GetIssueAsync(helper.Issue.Id))).Name);
        }

        [Test]
        public async Task IssueBlockedIfParentInNegotiation()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var child = await helper.CreateChild();
            await helper.Helper.SetStateOfIssue(child, State.NegotiationState);
            var childState = (await helper.Helper.GetStateById(await helper.GetIssueAsync(child.Id)));
            await helper.Helper.AcceptSummary(child.Id);
            Assert.AreEqual(State.BlockedState, (await helper.Helper.GetStateById(await helper.GetIssueAsync(child.Id))).Name);

            await helper.AcceptSummary();
            await helper.Helper.SetStateOfIssue(child, State.ProcessingState);
            Assert.AreEqual(State.ProcessingState, (await helper.Helper.GetStateById(await helper.GetIssueAsync(child.Id))).Name);
        }

        [Test]
        public async Task IssueBlockedIfParentInBlocked()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var predecessor = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.SetPredecessor(predecessor.Id);
            var child = await helper.CreateChild();
            
            await helper.Helper.SetStateOfIssue(child, State.NegotiationState);
            await helper.Helper.AcceptSummary(child.Id);
            Assert.AreEqual(State.BlockedState, (await helper.Helper.GetStateById(await helper.GetIssueAsync(child.Id))).Name);

            await helper.SetState(State.NegotiationState);
            await helper.Helper.SetStateOfIssue(predecessor, State.CancelledState);
            await helper.AcceptSummary();
            await helper.Helper.SetStateOfIssue(child, State.ProcessingState);
            Assert.AreEqual(State.ProcessingState, (await helper.Helper.GetStateById(await helper.GetIssueAsync(child.Id))).Name);
        }

        [Test]
        public async Task CannotMoveToProcessingPhaseIfRequirementsNotAccepted()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            await helper.SetState(State.NegotiationState);
            var res = await helper.SetState(State.ProcessingState);
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
        }
    }
}