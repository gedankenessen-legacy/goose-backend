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

namespace Goose.Tests.Application.IntegrationTests.issues
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    class IssueConversationTests
    {
        private sealed class TestScope : IDisposable
        {
            public HttpClient client;
            public WebApplicationFactory<Startup> _factory;
            public SimpleTestHelper Helper;
            public IIssueRequirementService _issueRequirementService;

            public TestScope()
            {
                Task.Run(() =>
                {
                    _factory = new WebApplicationFactory<Startup>();
                    client = _factory.CreateClient();

                    Helper = new SimpleTestHelperBuilder(client).Build().Result;
                    var scopeFactory = _factory.Server.Services.GetService<IServiceScopeFactory>();
                    using var scope = scopeFactory.CreateScope();
                    _issueRequirementService = scope.ServiceProvider.GetService<IIssueRequirementService>();
                }).Wait();
            }

            public void Dispose()
            {
                client?.Dispose();
                _factory?.Dispose();
                Helper.Dispose();
            }
        }

        [Test]
        public async Task PostConversation()
        {
            using (var scope = new TestScope())
            {
                var user = scope.Helper.User;

                var newItem = new IssueConversationDTO()
                {
                    Type = IssueConversation.MessageType,
                    Data = "TestConversation",
                };
                var issueId = scope.Helper.Issue.Id;
                var uri = $"/api/issues/{issueId}/conversations/";

                var response = await scope.client.PostAsync(uri, newItem.ToStringContent());
                Assert.IsTrue(response.IsSuccessStatusCode);

                var issue = await scope.Helper.GetIssueAsync(issueId);
                var latestConversationItem = issue.ConversationItems.Last();
                Assert.AreEqual(latestConversationItem.CreatorUserId, user.Id);
                Assert.AreEqual(latestConversationItem.Type, IssueConversation.MessageType);
                Assert.AreEqual(latestConversationItem.Data, "TestConversation");
            }
        }

        [Test]
        public async Task PostConversationWrongType()
        {
            using (var scope = new TestScope())
            {
                var newItem = new IssueConversationDTO()
                {
                    Type = IssueConversation.StateChangeType,
                    Data = "TestConversation",
                };

                var uri = $"/api/issues/{scope.Helper.Issue.Id}/conversations/";

                var response = await scope.client.PostAsync(uri, newItem.ToStringContent());
                Assert.IsFalse(response.IsSuccessStatusCode);

                var issue = await scope.Helper.GetIssueAsync(scope.Helper.Issue.Id);
                Assert.AreEqual(0, issue.ConversationItems.Count);
            }
        }

        [Test]
        public async Task PredecessorConversation()
        {
            using (var scope = new TestScope())
            {
                var issue = scope.Helper.Issue;
                var predecessorIssue = await scope.Helper.CreateIssue().Parse<IssueDTO>();

                var user = scope.Helper.User;

                var uri = $"/api/issues/{issue.Id}/predecessors/{predecessorIssue.Id}";

                // Add the predecessor
                var response = await scope.client.PutAsync(uri, null);
                Assert.IsTrue(response.IsSuccessStatusCode);

                var issueDTO = await scope.Helper.Helper.GetIssueThroughClientAsync(issue);
                var latestConversationItem = issueDTO.ConversationItems.Last();
                Assert.AreEqual(latestConversationItem.Creator.Id, user.Id);
                Assert.AreEqual(latestConversationItem.Type, IssueConversation.PredecessorAddedType);
                Assert.AreEqual(latestConversationItem.OtherTicketId, predecessorIssue.Id);
                Assert.IsTrue(latestConversationItem.Data.Contains(predecessorIssue.IssueDetail.Name));

                // Remove the predecessor
                response = await scope.client.DeleteAsync(uri);
                Assert.IsTrue(response.IsSuccessStatusCode);

                issueDTO = await scope.Helper.Helper.GetIssueThroughClientAsync(issue);
                latestConversationItem = issueDTO.ConversationItems.Last();
                Assert.AreEqual(latestConversationItem.Creator.Id, user.Id);
                Assert.AreEqual(latestConversationItem.Type, IssueConversation.PredecessorRemovedType);
                Assert.AreEqual(latestConversationItem.OtherTicketId, predecessorIssue.Id);
                Assert.IsTrue(latestConversationItem.Data.Contains(predecessorIssue.IssueDetail.Name));
            }
        }

        [Test]
        public async Task ChildConversation()
        {
            using (var scope = new TestScope())
            {
                var issue = scope.Helper.Issue;
                var childIssue = await scope.Helper.CreateIssue().Parse<IssueDTO>();
                
                var addUri = $"/api/issues/{childIssue.Id}/parent/{issue.Id}";
                var removeUri = $"/api/issues/{childIssue.Id}/parent";

                // Add the parent
                var response = await scope.client.PutAsync(addUri, null);
                Assert.IsTrue(response.IsSuccessStatusCode);

                var issueDTO = await scope.Helper.Helper.GetIssueThroughClientAsync(issue.Project.Id, issue.Id);
                var latestConversationItem = issueDTO.ConversationItems.Last();
                Assert.AreEqual(latestConversationItem.Creator.Id, scope.Helper.User.Id);
                Assert.AreEqual(latestConversationItem.Type, IssueConversation.ChildIssueAddedType);
                Assert.AreEqual(latestConversationItem.OtherTicketId, childIssue.Id);
                Assert.IsTrue(latestConversationItem.Data.Contains(childIssue.IssueDetail.Name));

                // Remove the parent
                response = await scope.client.DeleteAsync(removeUri);
                Assert.IsTrue(response.IsSuccessStatusCode);

                issueDTO = await scope.Helper.Helper.GetIssueThroughClientAsync(issue.Project.Id, issue.Id);
                latestConversationItem = issueDTO.ConversationItems.Last();
                Assert.AreEqual(latestConversationItem.Creator.Id, scope.Helper.User.Id);
                Assert.AreEqual(latestConversationItem.Type, IssueConversation.ChildIssueRemovedType);
                Assert.AreEqual(latestConversationItem.OtherTicketId, childIssue.Id);
                Assert.IsTrue(latestConversationItem.Data.Contains(childIssue.IssueDetail.Name));
            }
        }

        [Test]
        public async Task StatusConversation()
        {
            using (var scope = new TestScope())
            {
                var user = scope.Helper.User;
                var project = scope.Helper.Project;

                var issue = await scope.Helper.GetIssueAsync(scope.Helper.Issue.Id);
                var uri = $"/api/projects/{project.Id}/issues/{issue.Id}";

                var newState = await scope.Helper.Helper.GetStateByNameAsync(issue.ProjectId, State.NegotiationState);
                var issueDTO = new IssueDTO(issue, newState, project, user, user);

                var response = await scope.client.PutAsync(uri, issueDTO.ToStringContent());
                Assert.IsTrue(response.IsSuccessStatusCode);

                issue = await scope.Helper.GetIssueAsync(scope.Helper.Issue.Id);
                var latestConversationItem = issue.ConversationItems.Last();
                Assert.AreEqual(latestConversationItem.CreatorUserId, user.Id);
                Assert.AreEqual(latestConversationItem.Type, IssueConversation.StateChangeType);
                Assert.AreNotEqual(latestConversationItem.Data, "");
            }
        }

        [Test]
        public async Task SummaryAcceptedConversation()
        {
            using (var scope = new TestScope())
            {
                var issue = scope.Helper.Issue;
                IssueRequirement issueRequirement = new IssueRequirement() {Requirement = "Die Application Testen"};
                await scope._issueRequirementService.CreateAsync(issue.Id, issueRequirement);

                // Create a summary
                var uri = $"/api/issues/{issue.Id}/summaries";
                var response = await scope.client.PostAsync(uri, null);
                Assert.IsTrue(response.IsSuccessStatusCode);

                // Test if the SummaryCreated Conversation Item is there
                var newIssue = await scope.Helper.GetIssueAsync(issue.Id);
                var latestConversationItem = newIssue.ConversationItems.Last();
                Assert.AreEqual(latestConversationItem.CreatorUserId, scope.Helper.User.Id);
                Assert.AreEqual(latestConversationItem.Type, IssueConversation.SummaryCreatedType);
                Assert.AreEqual(latestConversationItem.Requirements.Single(), issueRequirement.Requirement);

                // Accept the summary
                uri = $"/api/issues/{issue.Id}/summaries?accept=true";
                response = await scope.client.PutAsync(uri, null);
                Assert.IsTrue(response.IsSuccessStatusCode);

                // Test if the SummaryDeclined Conversation Item is there
                newIssue = await scope.Helper.GetIssueAsync(issue.Id);
                latestConversationItem = newIssue.ConversationItems[newIssue.ConversationItems.Count - 2];
                Assert.IsTrue(latestConversationItem is not null);
                Assert.AreEqual(latestConversationItem.CreatorUserId, scope.Helper.User.Id);
                Assert.AreEqual(latestConversationItem.Type, IssueConversation.SummaryAcceptedType);
                Assert.AreEqual(latestConversationItem.Requirements.Single(), issueRequirement.Requirement);

                latestConversationItem = newIssue.ConversationItems[newIssue.ConversationItems.Count - 1];
                Assert.IsTrue(latestConversationItem is not null);
                Assert.AreEqual(latestConversationItem.CreatorUserId, scope.Helper.User.Id);
                Assert.AreEqual(latestConversationItem.Type, IssueConversation.StateChangeType);
                Assert.AreEqual(latestConversationItem.Data, $"Status von {State.NegotiationState} zu {State.WaitingState} geändert.");
            }
        }

        [Test]
        public async Task DeclineSummaryConversion()
        {
            using (var scope = new TestScope())
            {
                var user = scope.Helper.User;
                var issue = scope.Helper.Issue;
                
                IssueRequirement issueRequirement = new IssueRequirement() {Requirement = "Die Application Testen"};
                issueRequirement = await scope._issueRequirementService.CreateAsync(issue.Id, issueRequirement);

                // Create a summary
                var uri = $"/api/issues/{issue.Id}/summaries";
                var response = await scope.client.PostAsync(uri, null);
                Assert.IsTrue(response.IsSuccessStatusCode);

                // Test if the SummaryCreated Conversation Item is there
                var newIssue = await scope.Helper.GetIssueAsync(issue.Id);
                var latestConversationItem = newIssue.ConversationItems.Last();
                Assert.AreEqual(latestConversationItem.CreatorUserId, user.Id);
                Assert.AreEqual(latestConversationItem.Type, IssueConversation.SummaryCreatedType);
                Assert.AreEqual(latestConversationItem.Requirements.Single(), issueRequirement.Requirement);

                // Decline the summary
                uri = $"/api/issues/{issue.Id}/summaries?accept=false";
                response = await scope.client.PutAsync(uri, null);
                Assert.IsTrue(response.IsSuccessStatusCode);

                // Test if the SummaryDeclined Conversation Item is there
                newIssue = await scope.Helper.GetIssueAsync(issue.Id);
                latestConversationItem = newIssue.ConversationItems.Last();
                Assert.AreEqual(latestConversationItem.CreatorUserId, user.Id);
                Assert.AreEqual(latestConversationItem.Type, IssueConversation.SummaryDeclinedType);
                Assert.AreEqual(latestConversationItem.Requirements.Single(), issueRequirement.Requirement);
            }
        }
    }
}