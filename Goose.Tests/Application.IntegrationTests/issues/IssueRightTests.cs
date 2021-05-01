using Goose.API;
using Goose.API.Repositories;
using Goose.API.Services.Issues;
using Goose.Domain.Models.Auth;
using Goose.Domain.Models.Projects;
using Goose.Domain.Models.Issues;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Goose.Domain.Models.Identity;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    [TestFixture]
    [SingleThreaded]
    class IssueRightTests
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
            var signInResult = await TestHelper.Instance.GenerateCompany(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", signInResult.Token);
            await TestHelper.Instance.GenerateProject(_client);
            await TestHelper.Instance.AddUserToProject(_client, Role.ProjectLeaderRole);

            //Create 10 Issues with diffrent visibilities
            //i % 2 to set the visibility
            for (int i = 0; i < 10; i++)
                await TestHelper.Instance.GenerateIssue(_client, i, TestHelper.Instance.TicketNameProp + i, i % 2 == 0);
        }

        [TearDown]
        public async Task TearDown()
        {
            await TestHelper.Instance.ClearAll();
        }

        //Create Issue as Write Employee
        [Test]
        public async Task CreateIssueAsWriteEmployee()
        {
            await TestHelper.Instance.GenerateUserAndSetToProject(_client, Role.EmployeeRole);

            var postResult = await TestHelper.Instance.GenerateIssue(_client, 11, TestHelper.Instance.TicketNameProp + 11);

            Assert.IsTrue(postResult.IsSuccessStatusCode);
        }

        //Create Issue as Customer 
        [Test]
        public async Task CreateIssueAsCustomer()
        {
            await TestHelper.Instance.GenerateUserAndSetToProject(_client, Role.CustomerRole);

            var postResult = await TestHelper.Instance.GenerateIssue(_client, 11, TestHelper.Instance.TicketNameProp + 11);

            Assert.IsTrue(postResult.IsSuccessStatusCode);
        }

        //Create Issue as Read Only Employee (false)
        [Test]
        public async Task CreateIssueAsReadOnlyEmployee()
        {
            await TestHelper.Instance.GenerateUserAndSetToProject(_client, Role.ReadonlyEmployeeRole);

            var postResult = await TestHelper.Instance.GenerateIssue(_client, 11, TestHelper.Instance.TicketNameProp + 11);

            Assert.IsFalse(postResult.IsSuccessStatusCode);
        }

        //Update Issue as Write Employee
        [Test]
        public async Task UpdateIssueAsEmployee()
        {
            await TestHelper.Instance.GenerateUserAndSetToProject(_client, Role.EmployeeRole);

            var issue = await TestHelper.Instance.GetIssueAsync(0);

            issue.IssueDetail.Priority = 9;

            var state = await TestHelper.Instance.GetStateById(_client, issue.ProjectId, issue.StateId);
            var project = await TestHelper.Instance.GetProject();
            var client = await TestHelper.Instance.GetUserByUserId(issue.ClientId);
            var author = await TestHelper.Instance.GetUserByUserId(issue.AuthorId);

            var uri = $"api/projects/{issue.ProjectId}/issues/{issue.Id}";

            var responce = await _client.PutAsync(uri, new IssueDTO(issue, state, new ProjectDTO(project), new UserDTO(client), new UserDTO(author)).ToStringContent());

            Assert.IsTrue(responce.IsSuccessStatusCode);

            var issueUpdated = await TestHelper.Instance.GetIssueAsync(0);

            Assert.AreEqual(9, issueUpdated.IssueDetail.Priority);
        }

        //Update Issue as Customer 
        [Test]
        public async Task UpdateIssueAsCustomer()
        {
            await TestHelper.Instance.GenerateUserAndSetToProject(_client, Role.CustomerRole);

            var issue = await TestHelper.Instance.GetIssueAsync(0);

            issue.IssueDetail.Priority = 9;

            var state = await TestHelper.Instance.GetStateById(_client, issue.ProjectId, issue.StateId);
            var project = await TestHelper.Instance.GetProject();
            var client = await TestHelper.Instance.GetUserByUserId(issue.ClientId);
            var author = await TestHelper.Instance.GetUserByUserId(issue.AuthorId);

            var uri = $"api/projects/{issue.ProjectId}/issues/{issue.Id}";

            var responce = await _client.PutAsync(uri, new IssueDTO(issue, state, new ProjectDTO(project), new UserDTO(client), new UserDTO(author)).ToStringContent());

            Assert.IsTrue(responce.IsSuccessStatusCode);

            var issueUpdated = await TestHelper.Instance.GetIssueAsync(0);

            Assert.AreEqual(9, issueUpdated.IssueDetail.Priority);
        }

        //Update Issue as Read Only Employee (false)
        [Test]
        public async Task UpdateIssueAsReadOnlyEmployee()
        {
            await TestHelper.Instance.GenerateUserAndSetToProject(_client, Role.ReadonlyEmployeeRole);

            var issue = await TestHelper.Instance.GetIssueAsync(0);

            issue.IssueDetail.Priority = 9;

            var state = await TestHelper.Instance.GetStateById(_client, issue.ProjectId, issue.StateId);
            var project = await TestHelper.Instance.GetProject();
            var client = await TestHelper.Instance.GetUserByUserId(issue.ClientId);
            var author = await TestHelper.Instance.GetUserByUserId(issue.AuthorId);

            var uri = $"api/projects/{issue.ProjectId}/issues/{issue.Id}";

            var responce = await _client.PutAsync(uri, new IssueDTO(issue, state, new ProjectDTO(project), new UserDTO(client), new UserDTO(author)).ToStringContent());

            Assert.IsFalse(responce.IsSuccessStatusCode);
        }

        //Update state of Issue as Write Employee
        [Test]
        public async Task UpdateStateOfIssueAsEmployee()
        {
            await TestHelper.Instance.GenerateUserAndSetToProject(_client, Role.EmployeeRole);

            var issue = await TestHelper.Instance.GetIssueAsync(0);

            issue.IssueDetail.Priority = 9;

            var state = await TestHelper.Instance.GetStateByName(_client, issue.ProjectId, State.WaitingState);
            var project = await TestHelper.Instance.GetProject();
            var client = await TestHelper.Instance.GetUserByUserId(issue.ClientId);
            var author = await TestHelper.Instance.GetUserByUserId(issue.AuthorId);

            var uri = $"api/projects/{issue.ProjectId}/issues/{issue.Id}";

            var responce = await _client.PutAsync(uri, new IssueDTO(issue, state, new ProjectDTO(project), new UserDTO(client), new UserDTO(author)).ToStringContent());

            Assert.IsTrue(responce.IsSuccessStatusCode);

            var issueUpdated = await TestHelper.Instance.GetIssueAsync(0);

            Assert.AreEqual(state.Id, issueUpdated.StateId);
        }

        //Update state of Issue as Customer (false)
        [Test]
        public async Task UpdateStateOfIssueAsCustomer()
        {
            await TestHelper.Instance.GenerateUserAndSetToProject(_client, Role.CustomerRole);

            var issue = await TestHelper.Instance.GetIssueAsync(0);

            issue.IssueDetail.Priority = 9;

            var state = await TestHelper.Instance.GetStateByName(_client, issue.ProjectId, State.WaitingState);
            var project = await TestHelper.Instance.GetProject();
            var client = await TestHelper.Instance.GetUserByUserId(issue.ClientId);
            var author = await TestHelper.Instance.GetUserByUserId(issue.AuthorId);

            var uri = $"api/projects/{issue.ProjectId}/issues/{issue.Id}";

            var responce = await _client.PutAsync(uri, new IssueDTO(issue, state, new ProjectDTO(project), new UserDTO(client), new UserDTO(author)).ToStringContent());

            Assert.IsFalse(responce.IsSuccessStatusCode);

            var issueUpdated = await TestHelper.Instance.GetIssueAsync(0);
            var actualState = await TestHelper.Instance.GetStateByName(_client, issue.ProjectId, State.CheckingState);

            Assert.AreEqual(actualState.Id, issueUpdated.StateId);
        }

        //Update state of Issue as Read Only Employee (false)
        [Test]
        public async Task UpdateStateOfIssueAsReadOnlyEmployee()
        {
            await TestHelper.Instance.GenerateUserAndSetToProject(_client, Role.ReadonlyEmployeeRole);

            var issue = await TestHelper.Instance.GetIssueAsync(0);

            issue.IssueDetail.Priority = 9;

            var state = await TestHelper.Instance.GetStateByName(_client, issue.ProjectId, State.WaitingState);
            var project = await TestHelper.Instance.GetProject();
            var client = await TestHelper.Instance.GetUserByUserId(issue.ClientId);
            var author = await TestHelper.Instance.GetUserByUserId(issue.AuthorId);

            var uri = $"api/projects/{issue.ProjectId}/issues/{issue.Id}";

            var responce = await _client.PutAsync(uri, new IssueDTO(issue, state, new ProjectDTO(project), new UserDTO(client), new UserDTO(author)).ToStringContent());

            Assert.IsFalse(responce.IsSuccessStatusCode);

            var issueUpdated = await TestHelper.Instance.GetIssueAsync(0);
            var actualState = await TestHelper.Instance.GetStateByName(_client, issue.ProjectId, State.CheckingState);

            Assert.AreEqual(actualState.Id, issueUpdated.StateId);
        }

        //See Tickets as Leader (10)
        [Test]
        public async Task GetIssueAsLeader()
        {
            var project = await TestHelper.Instance.GetProject();
            var uri = $"api/projects/{project.Id}/issues";
            var responce = await _client.GetAsync(uri);
            var list = await responce.Content.Parse<IList<IssueDTO>>();
            Assert.AreEqual(10, list.Count);
        }

        //See Tickets as Write Employee (10)
        [Test]
        public async Task GetIssueAsEmployee()
        {
            await TestHelper.Instance.GenerateUserAndSetToProject(_client, Role.EmployeeRole);
            var project = await TestHelper.Instance.GetProject();
            var uri = $"api/projects/{project.Id}/issues";
            var responce = await _client.GetAsync(uri);
            var list = await responce.Content.Parse<IList<IssueDTO>>();
            Assert.AreEqual(10, list.Count);
        }

        //See Tickets as Customer (5)
        [Test]
        public async Task GetIssueAsCustomer()
        {
            await TestHelper.Instance.GenerateUserAndSetToProject(_client, Role.CustomerRole);
            var project = await TestHelper.Instance.GetProject();
            var uri = $"api/projects/{project.Id}/issues";
            var responce = await _client.GetAsync(uri);
            var list = await responce.Content.Parse<IList<IssueDTO>>();
            Assert.AreEqual(5, list.Count);
        }

        //See Tickets as Read Only Employee (10)
        [Test]
        public async Task GetIssueAsReadOnlyEmployee()
        {
            await TestHelper.Instance.GenerateUserAndSetToProject(_client, Role.ReadonlyEmployeeRole);
            var project = await TestHelper.Instance.GetProject();
            var uri = $"api/projects/{project.Id}/issues";
            var responce = await _client.GetAsync(uri);
            var list = await responce.Content.Parse<IList<IssueDTO>>();
            Assert.AreEqual(10, list.Count);
        }

        //Write a Message as Leader 
        [Test]
        public async Task WriteMessageAsLeader()
        {
            var user = await TestHelper.Instance.GetUser();

            var newItem = new IssueConversationDTO()
            {
                Type = IssueConversation.MessageType,
                Data = "TestConversation",
            };

            var issue = await TestHelper.Instance.GetIssueAsync(0);
            var uri = $"/api/issues/{issue.Id}/conversations/";

            var response = await _client.PostAsync(uri, newItem.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            var latestConversationItem = issue.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.CreatorUserId, user.Id);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.MessageType);
            Assert.AreEqual(latestConversationItem.Data, "TestConversation");
        }

        //Write a Message as Write Employee 
        [Test]
        public async Task WriteMessageAsEmployee()
        {
            var userId = await TestHelper.Instance.GenerateUserAndSetToProject(_client, Role.EmployeeRole);

            var newItem = new IssueConversationDTO()
            {
                Type = IssueConversation.MessageType,
                Data = "TestConversation",
            };

            var issue = await TestHelper.Instance.GetIssueAsync();
            var uri = $"/api/issues/{issue.Id}/conversations/";

            var response = await _client.PostAsync(uri, newItem.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            var latestConversationItem = issue.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.CreatorUserId, userId);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.MessageType);
            Assert.AreEqual(latestConversationItem.Data, "TestConversation");
        }

        //Write a Message as Customer 
        [Test]
        public async Task WriteMessageAsCustomer()
        {
            var userId = await TestHelper.Instance.GenerateUserAndSetToProject(_client, Role.CustomerRole);

            var newItem = new IssueConversationDTO()
            {
                Type = IssueConversation.MessageType,
                Data = "TestConversation",
            };

            var issue = await TestHelper.Instance.GetIssueAsync();
            var uri = $"/api/issues/{issue.Id}/conversations/";

            var response = await _client.PostAsync(uri, newItem.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            var latestConversationItem = issue.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.CreatorUserId, userId);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.MessageType);
            Assert.AreEqual(latestConversationItem.Data, "TestConversation");
        }

        //Write a Message as Read Only Employee (false)
        [Test]
        public async Task WriteMessageAsReadOnlyEmployee()
        {
            var userId = await TestHelper.Instance.GenerateUserAndSetToProject(_client, Role.ReadonlyEmployeeRole);

            var newItem = new IssueConversationDTO()
            {
                Type = IssueConversation.MessageType,
                Data = "TestConversation",
            };

            var issue = await TestHelper.Instance.GetIssueAsync();
            var uri = $"/api/issues/{issue.Id}/conversations/";

            var response = await _client.PostAsync(uri, newItem.ToStringContent());
            Assert.IsFalse(response.IsSuccessStatusCode);
        }
    }
}
