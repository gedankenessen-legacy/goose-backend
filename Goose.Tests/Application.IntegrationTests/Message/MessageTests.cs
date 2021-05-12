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
