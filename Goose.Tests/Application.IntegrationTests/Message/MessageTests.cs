using Goose.Domain.DTOs.Issues;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Goose.Domain.Models;
using Goose.Domain.DTOs;
using Goose.Domain.Models.Projects;
using Goose.Domain.Models.Issues;

namespace Goose.Tests.Application.IntegrationTests.Message
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    class MessageTests
    {
        private class SimpleTestHelperBuilderMessage : SimpleTestHelperBuilder
        {
            public override async Task<IssueDTO> CreateIssue(HttpClient client, SimpleTestHelper helper)
            {
                _issueDto.Author = helper.User;
                _issueDto.Client = helper.User;
                _issueDto.Project = helper.Project;
                _issueDto.IssueDetail.ExpectedTime = 1;
                return await helper.CreateIssue(_issueDto).Parse<IssueDTO>();
            }
        }

        [Test]
        public async Task CreateMessage()
        {
            using var helper = await new SimpleTestHelperBuilderMessage().Build();
            var message = GetValidMessage(helper);

            var uri = $"/api/messages";
            var responce = await helper.client.PostAsync(uri, message.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            uri = $"/api/messages/{helper.User.Id}";
            responce = await helper.client.GetAsync(uri);
            Assert.IsTrue(responce.IsSuccessStatusCode);
            var messageList = await responce.Parse<IList<MessageDTO>>();
            Assert.IsTrue(messageList.Count == 1);
        }

        [Test]
        public async Task UpdateMessage()
        {
            using var helper = await new SimpleTestHelperBuilderMessage().Build();
            var message = GetValidMessage(helper);

            var uri = $"/api/messages";
            var responce = await helper.client.PostAsync(uri, message.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            var newMessage = await responce.Parse<MessageDTO>();
            newMessage.Consented = true;
            uri = $"/api/messages/{newMessage.Id}";
            responce = await helper.client.PutAsync(uri, newMessage.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            var result = await responce.Parse<MessageDTO>();
            Assert.IsTrue(result.Consented);
        }

        [Test]
        public async Task DeleteMessage()
        {
            using var helper = await new SimpleTestHelperBuilderMessage().Build();
            var message = GetValidMessage(helper);

            var uri = $"/api/messages";
            var responce = await helper.client.PostAsync(uri, message.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            uri = $"/api/messages/{helper.User.Id}";
            responce = await helper.client.GetAsync(uri);
            Assert.IsTrue(responce.IsSuccessStatusCode);
            var messageList = await responce.Parse<IList<MessageDTO>>();
            Assert.IsTrue(messageList.Count == 1);

            uri = $"/api/messages/{messageList[0].Id}";
            responce = await helper.client.DeleteAsync(uri);
            Assert.IsTrue(responce.IsSuccessStatusCode);

            uri = $"/api/messages/{helper.User.Id}";
            responce = await helper.client.GetAsync(uri);
            Assert.IsTrue(responce.IsSuccessStatusCode);
            messageList = await responce.Parse<IList<MessageDTO>>();
            Assert.IsTrue(messageList.Count == 0);
        }

        [Test]
        public async Task TimeSheetMessageTest1()
        {
            using var helper = await new SimpleTestHelperBuilderMessage().Build();
            IssueTimeSheetDTO timeSheetDTO = new IssueTimeSheetDTO()
            {
                User = helper.User,
                Start = new DateTime(2021, 5, 13, 12, 29, 00)
            };

            var uri = $"/api/issues/{helper.Issue.Id}/timesheets";
            var responce = await helper.client.PostAsync(uri, timeSheetDTO.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);
            var result = await responce.Parse<IssueTimeSheetDTO>();

            uri = $"/api/issues/{helper.Issue.Id}/timesheets/{result.Id}";
            responce = await helper.client.GetAsync(uri);
            Assert.IsTrue(responce.IsSuccessStatusCode);
            var createdSheet = await responce.Parse<IssueTimeSheetDTO>();

            createdSheet.End = new DateTime(2021, 5, 13, 14, 29, 00);
            responce = await helper.client.PutAsync(uri, createdSheet.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            uri = $"/api/messages/{helper.User.Id}";
            responce = await helper.client.GetAsync(uri);
            Assert.IsTrue(responce.IsSuccessStatusCode);
            var messageList = await responce.Parse<IList<MessageDTO>>();

            Assert.IsTrue(messageList.Count == 1);
        }

        [Test]
        public async Task TimeSheetMessageTest2()
        {
            using var helper = await new SimpleTestHelperBuilderMessage().Build();

            //Create and end one Sheet without Exceeding expacted time
            IssueTimeSheetDTO timeSheetDTO = new IssueTimeSheetDTO()
            {
                User = helper.User,
                Start = new DateTime(2021, 5, 13, 12, 30, 00)
            };

            var uri = $"/api/issues/{helper.Issue.Id}/timesheets";
            var responce = await helper.client.PostAsync(uri, timeSheetDTO.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);
            var result = await responce.Parse<IssueTimeSheetDTO>();

            uri = $"/api/issues/{helper.Issue.Id}/timesheets/{result.Id}";
            responce = await helper.client.GetAsync(uri);
            Assert.IsTrue(responce.IsSuccessStatusCode);
            var createdSheet = await responce.Parse<IssueTimeSheetDTO>();

            createdSheet.End = new DateTime(2021, 5, 13, 13, 00, 00);
            responce = await helper.client.PutAsync(uri, createdSheet.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            uri = $"/api/messages/{helper.User.Id}";
            responce = await helper.client.GetAsync(uri);
            Assert.IsTrue(responce.IsSuccessStatusCode);
            var messageList = await responce.Parse<IList<MessageDTO>>();

            Assert.IsTrue(messageList.Count == 0);

            //Create Second Sheet which accours the Time Exceeding
            var timeSheetDTO2 = new IssueTimeSheetDTO()
            {
                User = helper.User,
                Start = new DateTime(2021, 5, 13, 13, 30, 00)
            };

            uri = $"/api/issues/{helper.Issue.Id}/timesheets";
            responce = await helper.client.PostAsync(uri, timeSheetDTO2.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);
            result = await responce.Parse<IssueTimeSheetDTO>();

            uri = $"/api/issues/{helper.Issue.Id}/timesheets/{result.Id}";
            responce = await helper.client.GetAsync(uri);
            Assert.IsTrue(responce.IsSuccessStatusCode);
            var createdSheet2 = await responce.Parse<IssueTimeSheetDTO>();

            createdSheet2.End = new DateTime(2021, 5, 13, 14, 30, 00);
            responce = await helper.client.PutAsync(uri, createdSheet2.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            uri = $"/api/messages/{helper.User.Id}";
            responce = await helper.client.GetAsync(uri);
            Assert.IsTrue(responce.IsSuccessStatusCode);
            var messageList2 = await responce.Parse<IList<MessageDTO>>();

            Assert.IsTrue(messageList2.Count == 1);
        }

        [Test]
        public async Task TimeSheetMessageTest3()
        {
            using var helper = await new SimpleTestHelperBuilderMessage().Build();
            IssueTimeSheetDTO timeSheetDTO = new IssueTimeSheetDTO()
            {
                User = helper.User,
                Start = new DateTime(2021, 5, 13, 12, 29, 00),
                End = new DateTime(2021, 5, 13, 14, 29, 00)
            };

            var uri = $"/api/issues/{helper.Issue.Id}/timesheets";
            var responce = await helper.client.PostAsync(uri, timeSheetDTO.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            uri = $"/api/messages/{helper.User.Id}";
            responce = await helper.client.GetAsync(uri);
            Assert.IsTrue(responce.IsSuccessStatusCode);
            var messageList = await responce.Parse<IList<MessageDTO>>();

            Assert.IsTrue(messageList.Count == 1);
        }

        [Test]
        public async Task IssueCanceledMesageTest()
        {
            using var helper = await new SimpleTestHelperBuilderMessage().Build();
            var user = helper.User;
            var project = helper.Project;

            var issue = await helper.Helper.GetIssueAsync(helper.Issue.Id);
            var uri = $"/api/projects/{project.Id}/issues/{issue.Id}";

            var newState = await helper.Helper.GetStateByNameAsync(issue.ProjectId, State.CancelledState);
            var issueDTO = new IssueDTO(issue, newState, project, user, user);

            var response = await helper.client.PutAsync(uri, issueDTO.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            uri = $"/api/messages/{helper.User.Id}";
            response = await helper.client.GetAsync(uri);
            Assert.IsTrue(response.IsSuccessStatusCode);
            var messageList = await response.Parse<IList<MessageDTO>>();

            Assert.IsTrue(messageList.Count == 2);
        }

        [Test]
        public async Task PostConversation()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();

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

            uri = $"/api/messages/{helper.User.Id}";
            response = await helper.client.GetAsync(uri);
            Assert.IsTrue(response.IsSuccessStatusCode);
            var messageList = await response.Parse<IList<MessageDTO>>();

            Assert.IsTrue(messageList.Count == 1);

        }

        private Domain.Models.Message GetValidMessage(SimpleTestHelper helper)
            => new Domain.Models.Message()
            {
                CompanyId = helper.Company.Id,
                ProjectId = helper.Project.Id,
                IssueId = helper.Issue.Id,
                ReceiverUserId = helper.User.Id,
                Type = MessageType.TimeExceeded,
                Consented = false
            };
    }
}
