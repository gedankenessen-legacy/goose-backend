﻿using Goose.API;
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
        private WebApplicationFactory<Startup> _factory;
        private ICompanyRepository _companyRepository;
        private IUserRepository _userRepository;
        private IIssueRepository _issueRepository;
        private IProjectRepository _projectRepository;
        private SignInResponse signInObject;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _factory = new WebApplicationFactory<Startup>();
            _client = _factory.CreateClient();
            var scopeFactory = _factory.Server.Services.GetService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope())
            {
                _companyRepository = scope.ServiceProvider.GetService<ICompanyRepository>();
                _userRepository = scope.ServiceProvider.GetService<IUserRepository>();
                _projectRepository = scope.ServiceProvider.GetService<IProjectRepository>();
                _issueRepository = scope.ServiceProvider.GetService<IIssueRepository>();
            }
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await Clear();
        }

        [SetUp]
        public async Task Setup()
        {
            await Clear();
            await Generate();
        }

        // TODO add tests
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

        private async Task Clear()
        {
            await TestHelper.Instance.ClearAll();
        }

        private async Task Generate()
        {
            signInObject = await TestHelper.Instance.GenerateCompany(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", signInObject.Token);
            await TestHelper.Instance.GenerateProject(_client);
            await TestHelper.Instance.GenerateIssue(_client);
        }
    }
}
