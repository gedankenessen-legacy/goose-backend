using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Issues;
using Goose.Domain.Models.Projects;
using MongoDB.Bson;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.Issues
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    class IssueRightTests
    {
        private class SimpleTestHelperBuilderIssueRights : SimpleTestHelperBuilder
        {
            public override async Task<IssueDTO> CreateIssue(HttpClient client, SimpleTestHelper helper)
            {
                var issue = await base.CreateIssue(client, helper);
                var tasks = new List<Task<HttpResponseMessage>>();
                for (int i = 0; i < 10; i++)
                {
                    var copy = issue.Copy();
                    copy.Id = ObjectId.Empty;
                    copy.IssueDetail.Visibility = i % 2 == 0;
                    tasks.Add(helper.CreateIssue(copy));
                }

                Task.WaitAll(tasks.ToArray());
                return await tasks[0].Result.Parse<IssueDTO>();
            }
        }

        //Create Issue as Write Employee
        [Test]
        public async Task CreateIssueAsWriteEmployee()
        {
            using var helper = await new SimpleTestHelperBuilderIssueRights().Build();
            await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.EmployeeRole);
            var postResult = await helper.CreateIssue();

            Assert.IsTrue(postResult.IsSuccessStatusCode);
        }

        //Create Issue as Customer 
        [Test]
        public async Task CreateIssueAsCustomer()
        {
            using var helper = await new SimpleTestHelperBuilderIssueRights().Build();
            await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.CustomerRole);
            var postResult = await helper.CreateIssue();

            Assert.IsTrue(postResult.IsSuccessStatusCode);
        }

        //Create Issue as Read Only Employee (false)
        [Test]
        public async Task CreateIssueAsReadOnlyEmployee()
        {
            using var helper = await new SimpleTestHelperBuilderIssueRights().Build();
            await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.ReadonlyEmployeeRole);
            var postResult = await helper.CreateIssue();

            Assert.IsFalse(postResult.IsSuccessStatusCode);
        }

        //Update Issue as Write Employee
        [Test]
        public async Task UpdateIssueAsEmployee()
        {
            using var helper = await new SimpleTestHelperBuilderIssueRights().Build();
            await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.EmployeeRole);

            var issue = helper.Issue.Copy();
            issue.IssueDetail.Priority = 9;

            var uri = $"api/projects/{issue.Project.Id}/issues/{issue.Id}";

            var response = await helper.client.PutAsync(uri, issue.ToStringContent());

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(9, (await helper.GetIssueAsync(issue.Id)).IssueDetail.Priority);
        }

        //Update Issue as Customer 
        [Test]
        public async Task UpdateIssueAsCustomer()
        {
            using var helper = await new SimpleTestHelperBuilderIssueRights().Build();
            await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.CustomerRole);

            var issue = helper.Issue.Copy();
            issue.IssueDetail.Priority = 9;

            var uri = $"api/projects/{issue.Project.Id}/issues/{issue.Id}";

            var response = await helper.client.PutAsync(uri, issue.ToStringContent());

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(9, (await helper.GetIssueAsync(issue.Id)).IssueDetail.Priority);
        }

        //Update Issue as Read Only Employee (false)
        [Test]
        public async Task UpdateIssueAsReadOnlyEmployee()
        {
            using var helper = await new SimpleTestHelperBuilderIssueRights().Build();
            await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.ReadonlyEmployeeRole);

            var issue = helper.Issue.Copy();
            issue.IssueDetail.Priority = 9;

            var uri = $"api/projects/{issue.Project.Id}/issues/{issue.Id}";

            var response = await helper.client.PutAsync(uri, issue.ToStringContent());

            Assert.IsFalse(response.IsSuccessStatusCode);
        }

        //Update state of Issue as Write Employee
        [Test]
        public async Task UpdateStateOfIssueAsEmployee()
        {
            using var helper = await new SimpleTestHelperBuilderIssueRights().Build();
            await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.EmployeeRole);

            var copy = helper.Issue.Copy();
            copy.State = await helper.Helper.GetStateByNameAsync(copy.Project.Id, State.WaitingState);

            var uri = $"api/projects/{copy.Project.Id}/issues/{copy.Id}";

            var response = await helper.client.PutAsync(uri, copy.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(copy.State.Id, (await helper.GetIssueAsync(copy.Id)).StateId);
        }

        //Update state of Issue as Customer (false)
        [Test]
        public async Task UpdateStateOfIssueAsCustomer()
        {
            using var helper = await new SimpleTestHelperBuilderIssueRights().Build();
            await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.CustomerRole);

            var copy = helper.Issue.Copy();
            copy.State = await helper.Helper.GetStateByNameAsync(copy.Project.Id, State.WaitingState);

            var uri = $"api/projects/{copy.Project.Id}/issues/{copy.Id}";

            var response = await helper.client.PutAsync(uri, copy.ToStringContent());
            Assert.IsFalse(response.IsSuccessStatusCode);
            Assert.AreEqual(helper.Issue.State.Id, (await helper.GetIssueAsync(copy.Id)).StateId);
        }

        [Test]
        public async Task UpdateStateOfIssueAsReadOnlyEmployee()
        {
            using var helper = await new SimpleTestHelperBuilderIssueRights().Build();
            await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.ReadonlyEmployeeRole);

            var copy = helper.Issue.Copy();
            copy.State = await helper.Helper.GetStateByNameAsync(copy.Project.Id, State.WaitingState);

            var uri = $"api/projects/{copy.Project.Id}/issues/{copy.Id}";

            var response = await helper.client.PutAsync(uri, copy.ToStringContent());
            Assert.IsFalse(response.IsSuccessStatusCode);
            Assert.AreEqual(helper.Issue.State.Id, (await helper.GetIssueAsync(copy.Id)).StateId);
        }

        //See Tickets as Leader (10)
        [Test]
        public async Task GetIssueAsLeader()
        {
            using var helper = await new SimpleTestHelperBuilderIssueRights().Build();
            var uri = $"api/projects/{helper.Project.Id}/issues";
            var response = await helper.client.GetAsync(uri);
            var list = await response.Content.Parse<IList<IssueDTO>>();
            Assert.AreEqual(11, list.Count);
        }

        //See Tickets as Write Employee (10)
        [Test]
        public async Task GetIssueAsEmployee()
        {
            using var helper = await new SimpleTestHelperBuilderIssueRights().Build();
            await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.EmployeeRole);
            var uri = $"api/projects/{helper.Project.Id}/issues";
            var response = await helper.client.GetAsync(uri);
            var list = await response.Content.Parse<IList<IssueDTO>>();
            Assert.AreEqual(11, list.Count);
        }

        //See Tickets as Customer (5)
        [Test]
        public async Task GetIssueAsCustomer()
        {
            using var helper = await new SimpleTestHelperBuilderIssueRights().Build();
            await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.CustomerRole);
            var uri = $"api/projects/{helper.Project.Id}/issues";
            var response = await helper.client.GetAsync(uri);
            var list = await response.Content.Parse<IList<IssueDTO>>();
            Assert.AreEqual(5, list.Count);
        }

        //See Tickets as Read Only Employee (10)
        [Test]
        public async Task GetIssueAsReadOnlyEmployee()
        {
            using var helper = await new SimpleTestHelperBuilderIssueRights().Build();
            await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.EmployeeRole);
            var uri = $"api/projects/{helper.Project.Id}/issues";
            var response = await helper.client.GetAsync(uri);
            var list = await response.Content.Parse<IList<IssueDTO>>();
            Assert.AreEqual(11, list.Count);
        }

        //Write a Message as Leader 
        [Test]
        public async Task WriteMessageAsLeader()
        {
            using var helper = await new SimpleTestHelperBuilderIssueRights().Build();
            var newItem = new IssueConversationDTO()
            {
                Type = IssueConversation.MessageType,
                Data = "TestConversation",
            };

            var uri = $"/api/issues/{helper.Issue.Id}/conversations/";

            var response = await helper.client.PostAsync(uri, newItem.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            var issue = await helper.GetIssueAsync(helper.Issue.Id);
            var latestConversationItem = issue.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.CreatorUserId, helper.User.Id);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.MessageType);
            Assert.AreEqual(latestConversationItem.Data, "TestConversation");
        }

        //Write a Message as Write Employee 
        [Test]
        public async Task WriteMessageAsEmployee()
        {
            using var helper = await new SimpleTestHelperBuilderIssueRights().Build();
            var userId = await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.EmployeeRole);

            var newItem = new IssueConversationDTO()
            {
                Type = IssueConversation.MessageType,
                Data = "TestConversation",
            };

            var uri = $"/api/issues/{helper.Issue.Id}/conversations/";

            var response = await helper.client.PostAsync(uri, newItem.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            var issue = await helper.GetIssueAsync(helper.Issue.Id);
            var latestConversationItem = issue.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.CreatorUserId, userId);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.MessageType);
            Assert.AreEqual(latestConversationItem.Data, "TestConversation");
        }

        //Write a Message as Customer 
        [Test]
        public async Task WriteMessageAsCustomer()
        {
            using var helper = await new SimpleTestHelperBuilderIssueRights().Build();
            var userId = await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.CustomerRole);

            var newItem = new IssueConversationDTO()
            {
                Type = IssueConversation.MessageType,
                Data = "TestConversation",
            };

            var uri = $"/api/issues/{helper.Issue.Id}/conversations/";

            var response = await helper.client.PostAsync(uri, newItem.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            var issue = await helper.GetIssueAsync(helper.Issue.Id);
            var latestConversationItem = issue.ConversationItems.Last();
            Assert.AreEqual(latestConversationItem.CreatorUserId, userId);
            Assert.AreEqual(latestConversationItem.Type, IssueConversation.MessageType);
            Assert.AreEqual(latestConversationItem.Data, "TestConversation");
        }

        //Write a Message as Read Only Employee (false)
        [Test]
        public async Task WriteMessageAsReadOnlyEmployee()
        {
            using var helper = await new SimpleTestHelperBuilderIssueRights().Build();
            await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.ReadonlyEmployeeRole);

            var newItem = new IssueConversationDTO()
            {
                Type = IssueConversation.MessageType,
                Data = "TestConversation",
            };

            var uri = $"/api/issues/{helper.Issue.Id}/conversations/";

            var response = await helper.client.PostAsync(uri, newItem.ToStringContent());
            Assert.IsFalse(response.IsSuccessStatusCode);
        }
    }
}