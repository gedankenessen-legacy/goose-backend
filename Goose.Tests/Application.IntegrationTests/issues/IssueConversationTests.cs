using Goose.API;
using Goose.API.Repositories;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Auth;
using Goose.Domain.Models.Issues;
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

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var factory = new WebApplicationFactory<Startup>();
            _client = factory.CreateClient();
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
            var newConversationItem = issue.ConversationItems.Last();
            Assert.AreEqual(newConversationItem.Type, IssueConversation.MessageType);
            Assert.AreEqual(newConversationItem.Data, "TestConversation");
        }
    }
}
