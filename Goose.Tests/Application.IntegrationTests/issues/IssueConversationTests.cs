using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Goose.API;
using Goose.API.Services.Issues;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Issues;
using Goose.Domain.Models.Projects;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.Issues
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    class IssueConversationTests
    {
        [Test]
        public async Task PostConversation()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            {
                var newItem = new IssueConversationDTO()
                {
                    Type = IssueConversation.MessageType,
                    Data = "TestConversation",
                };
                var issueId = helper.Issue.Id;
                var uri = $"/api/issues/{issueId}/conversations/";

                var response = await helper.client.PostAsync(uri, newItem.ToStringContent());
                Assert.IsTrue(response.IsSuccessStatusCode);

                var issue = await helper.Helper.GetIssueAsync(issueId);
                var latestConversationItem = issue.ConversationItems.Last();
                Assert.AreEqual(latestConversationItem.CreatorUserId, helper.User.Id);
                Assert.AreEqual(latestConversationItem.Type, IssueConversation.MessageType);
                Assert.AreEqual(latestConversationItem.Data, "TestConversation");
            }
        }

        [Test]
        public async Task PostConversationWrongType()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var newItem = new IssueConversationDTO()
            {
                Type = IssueConversation.StateChangeType,
                Data = "TestConversation",
            };

            var uri = $"/api/issues/{helper.Issue.Id}/conversations/";

            var response = await helper.client.PostAsync(uri, newItem.ToStringContent());
            Assert.IsFalse(response.IsSuccessStatusCode);

            var issue = await helper.Helper.GetIssueAsync(helper.Issue.Id);
            Assert.AreEqual(0, issue.ConversationItems.Count);
        }

        [Test]
        public async Task PredecessorConversation()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            {
                var predecessorIssue = await helper.CreateIssue().Parse<IssueDTO>();

                var uri = $"/api/issues/{helper.Issue.Id}/predecessors/{predecessorIssue.Id}";

                // Add the predecessor
                var response = await helper.client.PutAsync(uri, null);
                Assert.IsTrue(response.IsSuccessStatusCode);

                var issueDTO = await helper.Helper.GetIssueThroughClientAsync(helper.Issue);
                var latestConversationItem = issueDTO.ConversationItems.Last();
                Assert.AreEqual(latestConversationItem.Creator.Id, helper.User.Id);
                Assert.AreEqual(latestConversationItem.Type, IssueConversation.PredecessorAddedType);
                Assert.AreEqual(latestConversationItem.OtherTicketId, predecessorIssue.Id);
                Assert.IsTrue(latestConversationItem.Data.Contains(predecessorIssue.IssueDetail.Name));

                // Remove the predecessor
                response = await helper.client.DeleteAsync(uri);
                Assert.IsTrue(response.IsSuccessStatusCode);

                issueDTO = await helper.Helper.GetIssueThroughClientAsync(helper.Issue);
                latestConversationItem = issueDTO.ConversationItems.Last();
                Assert.AreEqual(latestConversationItem.Creator.Id, helper.User.Id);
                Assert.AreEqual(latestConversationItem.Type, IssueConversation.PredecessorRemovedType);
                Assert.AreEqual(latestConversationItem.OtherTicketId, predecessorIssue.Id);
                Assert.IsTrue(latestConversationItem.Data.Contains(predecessorIssue.IssueDetail.Name));
            }
        }

        [Test]
        public async Task ChildConversation()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var issue = helper.Issue;
            var childIssue = await helper.CreateIssue().Parse<IssueDTO>();

            var addUri = $"/api/issues/{childIssue.Id}/parent/{issue.Id}";
            var removeUri = $"/api/issues/{childIssue.Id}/parent";

            // Add the parent
            var response = await helper.client.PutAsync(addUri, null);
            Assert.IsTrue(response.IsSuccessStatusCode);

            var issueDTO = await helper.Helper.GetIssueThroughClientAsync(issue.Project.Id, issue.Id);
            var latestConversationItem = issueDTO.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.Creator.Id, helper.User.Id);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.ChildIssueAddedType);
            Assert.AreEqual(latestConversationItem.OtherTicketId, childIssue.Id);
            Assert.IsTrue(latestConversationItem.Data.Contains(childIssue.IssueDetail.Name));

            // Remove the parent
            response = await helper.client.DeleteAsync(removeUri);
            Assert.IsTrue(response.IsSuccessStatusCode);

            issueDTO = await helper.Helper.GetIssueThroughClientAsync(issue.Project.Id, issue.Id);
            latestConversationItem = issueDTO.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.Creator.Id, helper.User.Id);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.ChildIssueRemovedType);
            Assert.AreEqual(latestConversationItem.OtherTicketId, childIssue.Id);
            Assert.IsTrue(latestConversationItem.Data.Contains(childIssue.IssueDetail.Name));
        }

        [Test]
        public async Task StatusConversation()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            {
                var user = helper.User;
                var project = helper.Project;

                var issue = await helper.Helper.GetIssueAsync(helper.Issue.Id);
                var uri = $"/api/projects/{project.Id}/issues/{issue.Id}";

                var newState = await helper.Helper.GetStateByNameAsync(issue.ProjectId, State.NegotiationState);
                var issueDTO = new IssueDTO(issue, newState, project, user, user);

                var response = await helper.client.PutAsync(uri, issueDTO.ToStringContent());
                Assert.IsTrue(response.IsSuccessStatusCode);

                issue = await helper.GetIssueAsync(helper.Issue.Id);
                var latestConversationItem = issue.ConversationItems.Last();
                Assert.AreEqual(latestConversationItem.CreatorUserId, user.Id);
                Assert.AreEqual(latestConversationItem.Type, IssueConversation.StateChangeType);
                Assert.AreNotEqual(latestConversationItem.Data, "");
            }
        }

        [Test]
        public async Task SummaryAcceptedConversation()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var issue = helper.Issue;
            IssueRequirement issueRequirement = new IssueRequirement() {Requirement = "Die Application Testen"};
            await helper.Helper.IssueRequirementService.CreateAsync(issue.Id, issueRequirement);

            // Create a summary
            var uri = $"/api/issues/{issue.Id}/summaries";
            var response = await helper.client.PostAsync(uri, null);
            Assert.IsTrue(response.IsSuccessStatusCode);

            // Test if the SummaryCreated Conversation Item is there
            var newIssue = await helper.Helper.GetIssueAsync(issue.Id);
            var latestConversationItem = newIssue.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.CreatorUserId, helper.User.Id);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.SummaryCreatedType);
            Assert.AreEqual(latestConversationItem.Requirements.Single(), issueRequirement.Requirement);

            // Accept the summary
            uri = $"/api/issues/{issue.Id}/summaries?accept=true";
            response = await helper.client.PutAsync(uri, null);
            Assert.IsTrue(response.IsSuccessStatusCode);

            // Test if the SummaryDeclined Conversation Item is there
            newIssue = await helper.GetIssueAsync(issue.Id);
            latestConversationItem = newIssue.ConversationItems[newIssue.ConversationItems.Count - 2];
            Assert.IsTrue(latestConversationItem is not null);
            Assert.AreEqual(latestConversationItem.CreatorUserId, helper.User.Id);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.SummaryAcceptedType);
            Assert.AreEqual(latestConversationItem.Requirements.Single(), issueRequirement.Requirement);

            latestConversationItem = newIssue.ConversationItems[newIssue.ConversationItems.Count - 1];
            Assert.IsTrue(latestConversationItem is not null);
            Assert.AreEqual(latestConversationItem.CreatorUserId, helper.User.Id);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.StateChangeType);
            Assert.AreEqual(latestConversationItem.Data, $"Status von {State.NegotiationState} zu {State.WaitingState} geändert.");
        }

        [Test]
        public async Task DeclineSummaryConversion()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var user = helper.User;
            var issue = helper.Issue;

            IssueRequirement issueRequirement = new IssueRequirement() {Requirement = "Die Application Testen"};
            issueRequirement = await helper.Helper.IssueRequirementService.CreateAsync(issue.Id, issueRequirement);

            // Create a summary
            var uri = $"/api/issues/{issue.Id}/summaries";
            var response = await helper.client.PostAsync(uri, null);
            Assert.IsTrue(response.IsSuccessStatusCode);

            // Test if the SummaryCreated Conversation Item is there
            var newIssue = await helper.GetIssueAsync(issue.Id);
            var latestConversationItem = newIssue.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.CreatorUserId, user.Id);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.SummaryCreatedType);
            Assert.AreEqual(latestConversationItem.Requirements.Single(), issueRequirement.Requirement);

            // Decline the summary
            uri = $"/api/issues/{issue.Id}/summaries?accept=false";
            response = await helper.client.PutAsync(uri, null);
            Assert.IsTrue(response.IsSuccessStatusCode);

            // Test if the SummaryDeclined Conversation Item is there
            newIssue = await helper.Helper.GetIssueAsync(issue.Id);
            latestConversationItem = newIssue.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.CreatorUserId, user.Id);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.SummaryDeclinedType);
            Assert.AreEqual(latestConversationItem.Requirements.Single(), issueRequirement.Requirement);
        }
    }
}