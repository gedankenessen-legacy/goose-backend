using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Goose.API;
using Goose.API.Services.Issues;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Issues;
using Goose.Domain.Models.Projects;
using Microsoft.AspNetCore.Mvc.Testing;
using MongoDB.Bson;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.Issues
{
    [TestFixture]
    [SingleThreaded]
    public class IssuesControllerTests
    {
        private TestHelper _helper;
        private HttpClient _client;
        private IIssueRequirementService _issueRequirementService;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _helper = TestHelper.Instance;
            var factory = new WebApplicationFactory<Startup>();
            _client = factory.CreateClient();
        }

        [SetUp]
        public async Task Setup()
        {
            var signIn = await _helper.Login(_client);
            await _helper.GenerateProject(_client, signIn);
        }

        [TearDown]
        public async Task TearDown()
        {
            await TestHelper.Instance.ClearAll();
        }

        [Test]
        public async Task IssueCanOnlyHaveBugOrFeatureType()
        {
            var issue = await _helper.CreateDefaultIssue(_client);
            issue.IssueDetail.Type = "invalid-type";
            var res = await _helper.GenerateIssue(_client, issue);
            Assert.AreEqual(HttpStatusCode.BadRequest, res.Item1.StatusCode);
        }

        [Test]
        public async Task IssueDefaultStateIsCheckingState()
        {
            var issue = await _helper.CreateDefaultIssue(_client);
            issue.IssueDetail.RequirementsNeeded = true;
            issue.State = null;
            var res = await _helper.GenerateIssue(_client, issue);

            Assert.AreEqual(State.CheckingState, res.Item2?.State?.Name);
        }

        [Test]
        public async Task IssueDefaultStateIsProcessingStateIfSkipped()
        {
            var issue = await _helper.CreateDefaultIssue(_client);
            issue.IssueDetail.RequirementsNeeded = false;
            issue.State = null;
            var res = await _helper.GenerateIssue(_client, issue);

            Assert.AreEqual(State.ProcessingState, res.Item2?.State?.Name);
        }


        [Test]
        public async Task IssueCanOnlyBeCreatedWithAllRequiredValues()
        {
            await CreateRequestIsInvalid(nameof(IssueDTO.Project));
            await CreateRequestIsInvalid(nameof(IssueDTO.Client));
            await CreateRequestIsInvalid(nameof(IssueDTO.Author));
            await CreateRequestIsInvalid(nameof(IssueDTO.IssueDetail));
            await CreateRequestIsInvalid(nameOfIssueDetailProp: nameof(IssueDetail.Name));
            await CreateRequestIsInvalid(nameOfIssueDetailProp: nameof(IssueDetail.Type));
        }

        [Test]
        public async Task CannotUpdateInvalidFields()
        {
            var issue = await _helper.GenerateIssue(_client);

            await TryToUpdateSingleValue(issue, new UserDTO() {Id = ObjectId.Empty}, nameof(IssueDTO.Client));
            await TryToUpdateSingleValue(issue, new UserDTO() {Id = ObjectId.Empty}, nameof(IssueDTO.Author));
            await TryToUpdateSingleValue(issue, new ProjectDTO() {Id = ObjectId.Empty}, nameof(IssueDTO.Project));
            await TryToUpdateSingleValue(issue, true, nameOfIssueDetailProp: nameof(IssueDetail.Visibility));
            await TryToUpdateSingleValue(issue, null, nameOfIssueDetailProp: nameof(IssueDetail.Requirements));
            await TryToUpdateSingleValue(issue, null, nameOfIssueDetailProp: nameof(IssueDetail.RelevantDocuments));
            await TryToUpdateSingleValue(issue, "null", nameOfIssueDetailProp: nameof(IssueDetail.Type));
            await TryToUpdateSingleValue(issue, true, nameOfIssueDetailProp: nameof(IssueDetail.RequirementsNeeded));
            await TryToUpdateSingleValue(issue, true, nameOfIssueDetailProp: nameof(IssueDetail.RequirementsSummaryCreated));
            await TryToUpdateSingleValue(issue, true, nameOfIssueDetailProp: nameof(IssueDetail.RequirementsAccepted));
        }

        private async Task TryToUpdateSingleValue(IssueDTO dto, object value, string? nameOfProp = null, string? nameOfIssueDetailProp = null)
        {
            if ((nameOfProp == null && nameOfIssueDetailProp == null) || (nameOfProp != null && nameOfIssueDetailProp != null))
                return;

            var copy = dto.DeepClone();
            if (nameOfProp != null) copy.GetType().GetProperty(nameOfProp)?.SetValue(copy, value);
            if (nameOfIssueDetailProp != null) copy.IssueDetail.GetType().GetProperty(nameOfIssueDetailProp)?.SetValue(copy.IssueDetail, null);

            var uri = $"api/projects/{dto.Project.Id}/issues/{dto.Id}";
            dto.AssertEqualsJson(await (await _client.GetAsync(uri)).Parse<IssueDTO>());
        }

        /**
         * Creates an issue and sets a single value to null. Only nameOfProp or nameOfIssueDetailProp is allowed to be not null
         */
        private async Task CreateRequestIsInvalid(string? nameOfProp = null, string? nameOfIssueDetailProp = null, object value = null)
        {
            if ((nameOfProp == null && nameOfIssueDetailProp == null) || (nameOfProp != null && nameOfIssueDetailProp != null))
                return;
            var issue = await _helper.CreateDefaultIssue(_client);
            if (nameOfProp != null) issue.GetType().GetProperty(nameOfProp)?.SetValue(issue, value);
            if (nameOfIssueDetailProp != null) issue.IssueDetail.GetType().GetProperty(nameOfIssueDetailProp)?.SetValue(issue.IssueDetail, null);

            var res = await _helper.GenerateIssue(_client, issue);
            Assert.AreEqual(HttpStatusCode.BadRequest, res.Item1.StatusCode,
                $"Could create issue without required value {nameOfProp ?? nameOfIssueDetailProp}");
        }
    }
}