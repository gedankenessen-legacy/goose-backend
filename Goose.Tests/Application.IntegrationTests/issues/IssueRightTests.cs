using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Goose.API;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Issues;
using Goose.Domain.Models.Projects;
using Microsoft.AspNetCore.Mvc.Testing;
using MongoDB.Bson;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    class IssueRightTests
    {
        private sealed class TestScope : IDisposable
        {
            public HttpClient client;
            public WebApplicationFactory<Startup> _factory;
            public SimpleTestHelper Helper;

            public CompanyDTO Company;
            public ProjectDTO Project;

            public TestScope()
            {
                Task.Run(() =>
                {
                    _factory = new WebApplicationFactory<Startup>();
                    client = _factory.CreateClient();
                    Helper = new SimpleTestHelperBuilder(client).Build().Result;
                    Company = Helper.Company;
                    Project = Helper.Project;

                    var tasks = new List<Task<HttpResponseMessage>>();
                    for (int i = 0; i < 10; i++)
                    {
                        var copy = Helper.Issue.Copy();
                        copy.Id = ObjectId.Empty;
                        copy.IssueDetail.Visibility = i % 2 == 0;
                        tasks.Add(Helper.CreateIssue(copy));
                    }

                    Task.WaitAll(tasks.ToArray());
                }).Wait();
            }

            public void Dispose()
            {
                client?.Dispose();
                _factory?.Dispose();
                Helper.Dispose();
            }
        }

        //Create Issue as Write Employee
        [Test]
        public async Task CreateIssueAsWriteEmployee()
        {
            using (var scope = new TestScope())
            {
                await scope.Helper.Helper.GenerateUserAndSetToProject(scope.Company.Id, scope.Project.Id, Role.EmployeeRole);
                var postResult = await scope.Helper.CreateIssue();

                Assert.IsTrue(postResult.IsSuccessStatusCode);
            }
        }

        //Create Issue as Customer 
        [Test]
        public async Task CreateIssueAsCustomer()
        {
            using (var scope = new TestScope())
            {
                await scope.Helper.Helper.GenerateUserAndSetToProject(scope.Company.Id, scope.Project.Id, Role.CustomerRole);
                var postResult = await scope.Helper.CreateIssue();

                Assert.IsTrue(postResult.IsSuccessStatusCode);
            }
        }

        //Create Issue as Read Only Employee (false)
        [Test]
        public async Task CreateIssueAsReadOnlyEmployee()
        {
            using (var scope = new TestScope())
            {
                await scope.Helper.Helper.GenerateUserAndSetToProject(scope.Company.Id, scope.Project.Id, Role.ReadonlyEmployeeRole);
                var postResult = await scope.Helper.CreateIssue();

                Assert.IsFalse(postResult.IsSuccessStatusCode);
            }
        }

        //Update Issue as Write Employee
        [Test]
        public async Task UpdateIssueAsEmployee()
        {
            using (var scope = new TestScope())
            {
                await scope.Helper.Helper.GenerateUserAndSetToProject(scope.Company.Id, scope.Project.Id, Role.EmployeeRole);

                var issue = scope.Helper.Issue.Copy();
                issue.IssueDetail.Priority = 9;

                var uri = $"api/projects/{issue.Project.Id}/issues/{issue.Id}";

                var response = await scope.client.PutAsync(uri, issue.ToStringContent());

                Assert.IsTrue(response.IsSuccessStatusCode);
                Assert.AreEqual(9, (await scope.Helper.GetIssueAsync(issue.Id)).IssueDetail.Priority);
            }
        }

        //Update Issue as Customer 
        [Test]
        public async Task UpdateIssueAsCustomer()
        {
            using (var scope = new TestScope())
            {
                await scope.Helper.Helper.GenerateUserAndSetToProject(scope.Company.Id, scope.Project.Id, Role.CustomerRole);

                var issue = scope.Helper.Issue.Copy();
                issue.IssueDetail.Priority = 9;

                var uri = $"api/projects/{issue.Project.Id}/issues/{issue.Id}";

                var response = await scope.client.PutAsync(uri, issue.ToStringContent());

                Assert.IsTrue(response.IsSuccessStatusCode);
                Assert.AreEqual(9, (await scope.Helper.GetIssueAsync(issue.Id)).IssueDetail.Priority);
            }
        }

        //Update Issue as Read Only Employee (false)
        [Test]
        public async Task UpdateIssueAsReadOnlyEmployee()
        {
            using (var scope = new TestScope())
            {
                await scope.Helper.Helper.GenerateUserAndSetToProject(scope.Company.Id, scope.Project.Id, Role.ReadonlyEmployeeRole);

                var issue = scope.Helper.Issue.Copy();
                issue.IssueDetail.Priority = 9;

                var uri = $"api/projects/{issue.Project.Id}/issues/{issue.Id}";

                var response = await scope.client.PutAsync(uri, issue.ToStringContent());

                Assert.IsFalse(response.IsSuccessStatusCode);
            }
        }

        //Update state of Issue as Write Employee
        [Test]
        public async Task UpdateStateOfIssueAsEmployee()
        {
            using (var scope = new TestScope())
            {
                await scope.Helper.Helper.GenerateUserAndSetToProject(scope.Company.Id, scope.Project.Id, Role.EmployeeRole);

                var copy = scope.Helper.Issue.Copy();
                copy.State = await scope.Helper.Helper.GetStateByNameAsync(copy.Project.Id, State.WaitingState);

                var uri = $"api/projects/{copy.Project.Id}/issues/{copy.Id}";

                var response = await scope.client.PutAsync(uri, copy.ToStringContent());
                Assert.IsTrue(response.IsSuccessStatusCode);
                Assert.AreEqual(copy.State.Id, (await scope.Helper.GetIssueAsync(copy.Id)).StateId);
            }
        }

        //Update state of Issue as Customer (false)
        [Test]
        public async Task UpdateStateOfIssueAsCustomer()
        {
            using (var scope = new TestScope())
            {
                await scope.Helper.Helper.GenerateUserAndSetToProject(scope.Company.Id, scope.Project.Id, Role.CustomerRole);

                var copy = scope.Helper.Issue.Copy();
                copy.State = await scope.Helper.Helper.GetStateByNameAsync(copy.Project.Id, State.WaitingState);

                var uri = $"api/projects/{copy.Project.Id}/issues/{copy.Id}";

                var response = await scope.client.PutAsync(uri, copy.ToStringContent());
                Assert.IsFalse(response.IsSuccessStatusCode);
                Assert.AreEqual(scope.Helper.Issue.State.Id, (await scope.Helper.GetIssueAsync(copy.Id)).StateId);
            }
        }

        [Test]
        public async Task UpdateStateOfIssueAsReadOnlyEmployee()
        {
            using (var scope = new TestScope())
            {
                await scope.Helper.Helper.GenerateUserAndSetToProject(scope.Company.Id, scope.Project.Id, Role.ReadonlyEmployeeRole);

                var copy = scope.Helper.Issue.Copy();
                copy.State = await scope.Helper.Helper.GetStateByNameAsync(copy.Project.Id, State.WaitingState);

                var uri = $"api/projects/{copy.Project.Id}/issues/{copy.Id}";

                var response = await scope.client.PutAsync(uri, copy.ToStringContent());
                Assert.IsFalse(response.IsSuccessStatusCode);
                Assert.AreEqual(scope.Helper.Issue.State.Id, (await scope.Helper.GetIssueAsync(copy.Id)).StateId);
            }
        }

        //See Tickets as Leader (10)
        [Test]
        public async Task GetIssueAsLeader()
        {
            using (var scope = new TestScope())
            {
                var uri = $"api/projects/{scope.Project.Id}/issues";
                var response = await scope.client.GetAsync(uri);
                var list = await response.Content.Parse<IList<IssueDTO>>();
                Assert.AreEqual(11, list.Count);
            }
        }

        //See Tickets as Write Employee (10)
        [Test]
        public async Task GetIssueAsEmployee()
        {
            using (var scope = new TestScope())
            {
                await scope.Helper.Helper.GenerateUserAndSetToProject(scope.Company.Id, scope.Project.Id, Role.EmployeeRole);
                var uri = $"api/projects/{scope.Project.Id}/issues";
                var response = await scope.client.GetAsync(uri);
                var list = await response.Content.Parse<IList<IssueDTO>>();
                Assert.AreEqual(11, list.Count);
            }
        }

        //See Tickets as Customer (5)
        [Test]
        public async Task GetIssueAsCustomer()
        {
            using (var scope = new TestScope())
            {
                await scope.Helper.Helper.GenerateUserAndSetToProject(scope.Company.Id, scope.Project.Id, Role.CustomerRole);
                var uri = $"api/projects/{scope.Project.Id}/issues";
                var response = await scope.client.GetAsync(uri);
                var list = await response.Content.Parse<IList<IssueDTO>>();
                Assert.AreEqual(5, list.Count);
            }
        }

        //See Tickets as Read Only Employee (10)
        [Test]
        public async Task GetIssueAsReadOnlyEmployee()
        {
            using (var scope = new TestScope())
            {
                await scope.Helper.Helper.GenerateUserAndSetToProject(scope.Company.Id, scope.Project.Id, Role.EmployeeRole);
                var uri = $"api/projects/{scope.Project.Id}/issues";
                var response = await scope.client.GetAsync(uri);
                var list = await response.Content.Parse<IList<IssueDTO>>();
                Assert.AreEqual(11, list.Count);
            }
        }

        //Write a Message as Leader 
        [Test]
        public async Task WriteMessageAsLeader()
        {
            using (var scope = new TestScope())
            {
                var newItem = new IssueConversationDTO()
                {
                    Type = IssueConversation.MessageType,
                    Data = "TestConversation",
                };

                var uri = $"/api/issues/{scope.Helper.Issue.Id}/conversations/";

                var response = await scope.client.PostAsync(uri, newItem.ToStringContent());
                Assert.IsTrue(response.IsSuccessStatusCode);

                var issue = await scope.Helper.GetIssueAsync(scope.Helper.Issue.Id);
                var latestConversationItem = issue.ConversationItems.Last();
                Assert.AreEqual(latestConversationItem.CreatorUserId, scope.Helper.User.Id);
                Assert.AreEqual(latestConversationItem.Type, IssueConversation.MessageType);
                Assert.AreEqual(latestConversationItem.Data, "TestConversation");
            }
        }

        //Write a Message as Write Employee 
        [Test]
        public async Task WriteMessageAsEmployee()
        {
            using (var scope = new TestScope())
            {
                var userId = await scope.Helper.Helper.GenerateUserAndSetToProject(scope.Company.Id, scope.Project.Id, Role.EmployeeRole);

                var newItem = new IssueConversationDTO()
                {
                    Type = IssueConversation.MessageType,
                    Data = "TestConversation",
                };

                var uri = $"/api/issues/{scope.Helper.Issue.Id}/conversations/";

                var response = await scope.client.PostAsync(uri, newItem.ToStringContent());
                Assert.IsTrue(response.IsSuccessStatusCode);

                var issue = await scope.Helper.GetIssueAsync(scope.Helper.Issue.Id);
                var latestConversationItem = issue.ConversationItems.Last();
                Assert.AreEqual(latestConversationItem.CreatorUserId, userId);
                Assert.AreEqual(latestConversationItem.Type, IssueConversation.MessageType);
                Assert.AreEqual(latestConversationItem.Data, "TestConversation");
            }
        }

        //Write a Message as Customer 
        [Test]
        public async Task WriteMessageAsCustomer()
        {
            using (var scope = new TestScope())
            {
                var userId = await scope.Helper.Helper.GenerateUserAndSetToProject(scope.Company.Id, scope.Project.Id, Role.CustomerRole);

                var newItem = new IssueConversationDTO()
                {
                    Type = IssueConversation.MessageType,
                    Data = "TestConversation",
                };

                var uri = $"/api/issues/{scope.Helper.Issue.Id}/conversations/";

                var response = await scope.client.PostAsync(uri, newItem.ToStringContent());
                Assert.IsTrue(response.IsSuccessStatusCode);

                var issue = await scope.Helper.GetIssueAsync(scope.Helper.Issue.Id);
                var latestConversationItem = issue.ConversationItems.Last();
                Assert.AreEqual(latestConversationItem.CreatorUserId, userId);
                Assert.AreEqual(latestConversationItem.Type, IssueConversation.MessageType);
                Assert.AreEqual(latestConversationItem.Data, "TestConversation");
            }
        }

        //Write a Message as Read Only Employee (false)
        [Test]
        public async Task WriteMessageAsReadOnlyEmployee()
        {
            using (var scope = new TestScope())
            {
                await scope.Helper.Helper.GenerateUserAndSetToProject(scope.Company.Id, scope.Project.Id, Role.ReadonlyEmployeeRole);

                var newItem = new IssueConversationDTO()
                {
                    Type = IssueConversation.MessageType,
                    Data = "TestConversation",
                };

                var uri = $"/api/issues/{scope.Helper.Issue.Id}/conversations/";

                var response = await scope.client.PostAsync(uri, newItem.ToStringContent());
                Assert.IsFalse(response.IsSuccessStatusCode);
            }
        }
    }
}