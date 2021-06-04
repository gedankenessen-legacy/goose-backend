using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Resources;
using System.Threading.Tasks;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Projects;
using Goose.Tests.Application.IntegrationTests.Issues;
using Microsoft.AspNetCore.Http;
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
                issue.State = state;
                var res = await helper.Helper.UpdateIssue(issue);
                Assert.AreEqual(HttpStatusCode.NoContent, res.StatusCode);
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

            await helper.SetState(State.NegotiationState);
            await helper.SetState(State.ProcessingState);

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
            await helper.Helper.SetStateOfIssue(issue, State.NegotiationState);
            await helper.Helper.SetStateOfIssue(issue, State.ProcessingState);

            var newIssue = await helper.GetIssueAsync(issue.Id);
            Assert.AreEqual(State.WaitingState, (await helper.Helper.GetStateById(newIssue)).Name);
        }

        [Test]
        public async Task ParentIsBlockedIfChildAddedInPrecessingPhase()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            await helper.SetState(State.NegotiationState);
            await helper.SetState(State.ProcessingState);
            var issue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual(State.ProcessingState, (await helper.Helper.GetStateById(issue.ProjectId, issue.StateId)).Name);
            await helper.CreateChild();
            issue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual(State.BlockedState, (await helper.Helper.GetStateById(issue.ProjectId, issue.StateId)).Name);
        }
        [Test]
        public async Task ParentIsBlockedIfPredecessorAddedInPrecessingPhase()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var predecessor = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.SetState(State.NegotiationState);
            await helper.SetState(State.ProcessingState);
            var issue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual(State.ProcessingState, (await helper.Helper.GetStateById(issue.ProjectId, issue.StateId)).Name);
            await helper.SetPredecessor(predecessor.Id);
            issue = await helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual(State.BlockedState, (await helper.Helper.GetStateById(issue.ProjectId, issue.StateId)).Name);
        }
    }
}