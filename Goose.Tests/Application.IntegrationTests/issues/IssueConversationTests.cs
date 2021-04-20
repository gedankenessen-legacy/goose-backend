using Goose.API;
using Goose.API.Repositories;
using Goose.API.Services.Issues;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Auth;
using Goose.Domain.Models.Issues;
using Goose.Domain.Models.Projects;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    [TestFixture]
    class IssueConversationTests
    {
        private HttpClient _client;
        private IIssueRequirementService _issueRequirementService;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var factory = new WebApplicationFactory<Startup>();
            _client = factory.CreateClient();

            var scopeFactory = factory.Server.Services.GetService<IServiceScopeFactory>();

            using var scope = scopeFactory.CreateScope();
            _issueRequirementService = scope.ServiceProvider.GetService<IIssueRequirementService>();
        }

        [SetUp]
        public async Task Setup()
        {
            await TestHelper.Instance.GenerateAll(_client);
        }

        [TearDown]
        public async Task Teardown()
        {
            await TestHelper.Instance.ClearAll();
        }

        [Test]
        public async Task PostConversation()
        {
            var user = await TestHelper.Instance.GetUser();

            var newItem = new IssueConversationDTO()
            {
                Creator = new Domain.DTOs.UserDTO(user),
                Type = IssueConversation.MessageType,
                Data = "TestConversation",
            };

            var issue = await TestHelper.Instance.GetIssueAsync();
            var uri = $"/api/issues/{issue.Id}/conversations/";

            var response = await _client.PostAsync(uri, newItem.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            var latestConversationItem = issue.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.CreatorUserId, user.Id);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.MessageType);
            Assert.AreEqual(latestConversationItem.Data, "TestConversation");
        }

        [Test]
        public async Task PostConversationWrongType()
        {
            var user = await TestHelper.Instance.GetUser();

            var newItem = new IssueConversationDTO()
            {
                Creator = new Domain.DTOs.UserDTO(user),
                Type = IssueConversation.StateChangeType,
                Data = "TestConversation",
            };

            var issue = await TestHelper.Instance.GetIssueAsync();
            var uri = $"/api/issues/{issue.Id}/conversations/";

            var response = await _client.PostAsync(uri, newItem.ToStringContent());
            Assert.IsFalse(response.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            Assert.AreEqual(0, issue.ConversationItems.Count);
        }

        [Test]
        public async Task PredecessorConversation()
        {
            await TestHelper.Instance.GenerateIssue(_client, 1);
            var predecessorIssue = await TestHelper.Instance.GetIssueAsync(1);

            var user = await TestHelper.Instance.GetUser();

            var issue = await TestHelper.Instance.GetIssueAsync();
            var uri = $"/api/issues/{issue.Id}/predecessors/{predecessorIssue.Id}";

            // Add the predecessor
            var response = await _client.PutAsync(uri, null);
            Assert.IsTrue(response.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            var latestConversationItem = issue.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.CreatorUserId, user.Id);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.PredecessorAddedType);
            Assert.AreEqual(latestConversationItem.Data, predecessorIssue.Id.ToString());

            // Remove the predecessor
            response = await _client.DeleteAsync(uri);
            Assert.IsTrue(response.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            latestConversationItem = issue.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.CreatorUserId, user.Id);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.PredecessorRemovedType);
            Assert.AreEqual(latestConversationItem.Data, predecessorIssue.Id.ToString());
        }

        [Test]
        public async Task ChildConversation()
        {
            await TestHelper.Instance.GenerateIssue(_client, 1);
            var childIssue = await TestHelper.Instance.GetIssueAsync(1);

            var user = await TestHelper.Instance.GetUser();

            var issue = await TestHelper.Instance.GetIssueAsync();
            var addUri = $"/api/issues/{childIssue.Id}/parent/{issue.Id}";
            var removeUri = $"/api/issues/{childIssue.Id}/parent";

            // Add the parent
            var response = await _client.PutAsync(addUri, null);
            Assert.IsTrue(response.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            var latestConversationItem = issue.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.CreatorUserId, user.Id);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.ChildIssueAddedType);
            Assert.AreEqual(latestConversationItem.Data, childIssue.Id.ToString());

            // Remove the parent
            response = await _client.DeleteAsync(removeUri);
            Assert.IsTrue(response.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            latestConversationItem = issue.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.CreatorUserId, user.Id);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.ChildIssueRemovedType);
            Assert.AreEqual(latestConversationItem.Data, childIssue.Id.ToString());
        }

        [Test]
        public async Task StatusConversation()
        {
            var user = await TestHelper.Instance.GetUser();
            var project = await TestHelper.Instance.GetProject();

            var issue = await TestHelper.Instance.GetIssueAsync();
            var uri = $"/api/projects/{project.Id}/issues/{issue.Id}";

            var newState = await TestHelper.Instance.GetStateByName(_client, issue.ProjectId, State.NegotiationState);
            var userDTO = new UserDTO(user);
            var issueDTO = new IssueDTO(issue, newState, new ProjectDTO(project), userDTO, userDTO);

            var response = await _client.PutAsync(uri, issueDTO.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            var latestConversationItem = issue.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.CreatorUserId, user.Id);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.StateChangeType);
            Assert.AreNotEqual(latestConversationItem.Data, "");
        }

        [Test]
        public async Task SummaryAcceptedConversation()
        {
            var user = await TestHelper.Instance.GetUser();

            var issue = await TestHelper.Instance.GetIssueAsync();
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            await _issueRequirementService.CreateAsync(issue.Id, issueRequirement);

            // Create a summary
            var uri = $"/api/issues/{issue.Id}/summaries";
            var response = await _client.PostAsync(uri, null);
            Assert.IsTrue(response.IsSuccessStatusCode);

            // Test if the SummaryCreated Conversation Item is there
            issue = await TestHelper.Instance.GetIssueAsync();
            var latestConversationItem = issue.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.CreatorUserId, user.Id);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.SummaryCreatedType);
            Assert.AreEqual(latestConversationItem.RequirementIds.Single(), issueRequirement.Id);

            // Accept the summary
            uri = $"/api/issues/{issue.Id}/summaries?accept=true";
            response = await _client.PutAsync(uri, null);
            Assert.IsTrue(response.IsSuccessStatusCode);

            // Test if the SummaryDeclined Conversation Item is there
            issue = await TestHelper.Instance.GetIssueAsync();
            latestConversationItem = issue.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.CreatorUserId, user.Id);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.SummaryAcceptedType);
            Assert.AreEqual(latestConversationItem.RequirementIds.Single(), issueRequirement.Id);
        }

        [Test]
        public async Task DeclineSummaryConversion()
        {
            var user = await TestHelper.Instance.GetUser();

            var issue = await TestHelper.Instance.GetIssueAsync();
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            issueRequirement = await _issueRequirementService.CreateAsync(issue.Id, issueRequirement);

            // Create a summary
            var uri = $"/api/issues/{issue.Id}/summaries";
            var response = await _client.PostAsync(uri, null);
            Assert.IsTrue(response.IsSuccessStatusCode);

            // Test if the SummaryCreated Conversation Item is there
            issue = await TestHelper.Instance.GetIssueAsync();
            var latestConversationItem = issue.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.CreatorUserId, user.Id);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.SummaryCreatedType);
            Assert.AreEqual(latestConversationItem.RequirementIds.Single(), issueRequirement.Id);

            // Decline the summary
            uri = $"/api/issues/{issue.Id}/summaries?accept=false";
            response = await _client.PutAsync(uri, null);
            Assert.IsTrue(response.IsSuccessStatusCode);

            // Test if the SummaryDeclined Conversation Item is there
            issue = await TestHelper.Instance.GetIssueAsync();
            latestConversationItem = issue.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.CreatorUserId, user.Id);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.SummaryDeclinedType);
            Assert.AreEqual(latestConversationItem.RequirementIds.Single(), issueRequirement.Id);
        }
    }
}
