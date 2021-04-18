using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Goose.API;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Companies;
using Goose.Domain.Models.Issues;
using Microsoft.AspNetCore.Mvc.Testing;
using MongoDB.Bson;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.Issues
{
    /**
     * The issue endpoint is dependant of the State, Company, project and User controller. Tests may fail if those weren't tested first.
     */
    /*[TestFixture]
    [SingleThreaded]
    public class IssuesControllerTests
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;

        private CompanyDTO _company { get; set; }
        private ProjectDTO _project { get; set; }
        private StateDTO _state { get; set; }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new WebApplicationFactory<Startup>();
            _client = _factory.CreateClient();
        }

        [SetUp]
        public async Task Setup()
        {
            ClearDatabase();
            _company = await _client.CreateCompany();
            _project = await _client.CreateProject(_company.Id);
            _state = await _client.CreateProjectState(_project.Id);
        }

        [Test]
        public async Task GetEmptyListOfIssuesFromProject()
        {
            var uri = $"api/projects/{_project.Id}/issues";
            var response = await _client.GetAsync(uri).PrintMessage();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.Parse<List<IssueDTODetailed>>();
            Assert.IsEmpty(content, "Result is Empty");
        }

        [Test]
        public async Task GetIssuesOfNotExistingProject()
        {
            var uri = $"api/projects/{ObjectId.Empty}/issues";
            var response = await _client.GetAsync(uri).PrintMessage();
            ;
            var content = await response.Content.Parse<List<IssueDTODetailed>>();
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task GetNotExistingIssueOfExistingProject()
        {
            var uri = $"api/projects/{_project.Id}/issues/{ObjectId.Empty}";
            var response = await _client.GetAsync(uri).PrintMessage();
            ;
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task GetNotExistingIssueOfNotExistingProject()
        {
            var uri = $"api/projects/{ObjectId.Empty}/issues/{ObjectId.Empty}";
            var response = await _client.GetAsync(uri).PrintMessage();
            var body = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }


        [Test]
        public async Task CreateIssue()
        {
            var response = await _createDefaultIssue().PrintMessage();
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            var content = await response.Content.Parse<IssueDTO>();
            Assert.IsNotNull(content.Id);
        }

        [Test]
        public async Task CreateIssueOfNotExistingProject()
        {
            var issue = new IssueDTO
            {
                Author = _company.User.User,
                Client = _company.User.User,
                Project = _project,
                State = _state,
                IssueDetail = new IssueDetail
                {
                    Name = null,
                    Type = null,
                    StartDate = default,
                    EndDate = default,
                    ExpectedTime = 0,
                    Progress = 0,
                    Description = null,
                    Requirements = null,
                    RequirementsAccepted = false,
                    RequirementsSummaryCreated = false,
                    RequirementsNeeded = false,
                    Priority = 0,
                    Visibility = false,
                    RelevantDocuments = null
                }
            };
            var id = ObjectId.GenerateNewId();
            issue.Project.Id = id;
            var uri = $"api/projects/{id}/issues";
            var response = await _client.PostAsync(uri, issue.ToStringContent()).PrintMessage();
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task CannotCreateIssueWithMissingValue()
        {
            //TODO welche felder müssen befüllt sein. es müssen alle getestet werden
            var response = await _createDefaultIssue().PrintMessage();
            var content = await response.Content.Parse<IssueDTO>();
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.IsNotNull(content.Id);
        }

        //TODO fehlerhafte werte testen. beispiel enddate ist vor startdate

        [Test]
        public async Task GetExistingIssueFromExistingProject()
        {
            var issue = await (await _createDefaultIssue()).Content.Parse<IssueDTO>();
            var uri = $"api/projects/{_project.Id}/issues/{issue.Id}";
            var response = await _client.GetAsync(uri).PrintMessage();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.Parse<IssueDTO>();
            Assert.AreEqual(issue.Id, content.Id);
        }

        [Test]
        public async Task GetExistingIssueFromNotExistingProject()
        {
            var issue = await (await _createDefaultIssue()).Content.Parse<IssueDTO>();
            var uri = $"api/projects/{ObjectId.Empty}/issues/{issue.Id}";
            var response = await _client.GetAsync(uri).PrintMessage();
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task<HttpResponseMessage> _createDefaultIssue()
        {
            var issue = new IssueDTO
            {
                Author = _company.User.User,
                Client = _company.User.User,
                Project = _project,
                State = _state,
                IssueDetail = new IssueDetail
                {
                    Name = null,
                    Type = null,
                    StartDate = default,
                    EndDate = default,
                    ExpectedTime = 0,
                    Progress = 0,
                    Description = null,
                    Requirements = null,
                    RequirementsAccepted = false,
                    RequirementsSummaryCreated = false,
                    RequirementsNeeded = false,
                    Priority = 0,
                    Visibility = false,
                    RelevantDocuments = null
                }
            };
            var uri = $"api/projects/{_project.Id}/issues";
            return await _client.PostAsync(uri, issue.ToStringContent());
        }

        private void ClearDatabase()
        {
        }
    }


    static class IssueControllerTestsUtil
    {
        public static async Task<HttpResponseMessage> PrintMessage(this Task<HttpResponseMessage> response)
        {
            try
            {
                Console.WriteLine(await (await response).Content.ReadAsStringAsync());
            }
            catch (Exception)
            {
                // ignored
            }

            return await response;
        }

        public static void EqualJson(this object expected, object other)
        {
            Assert.AreEqual(expected.ToJson(), other.ToJson());
        }

        public static async Task<CompanyDTO> CreateCompany(this HttpClient client)
        {
            var uri = "api/companies";
            var body = new CompanyLogin
            {
                CompanyName = $"Company {new Random().NextDouble()}",
                HashedPassword = "xxx"
            };
            var response = await client.PostAsync(uri, body.ToStringContent());
            return await response.Content.Parse<CompanyDTO>();
        }

        public static async Task<ProjectDTO> CreateProject(this HttpClient client, ObjectId companyId)
        {
            var uri = $"api/companies/{companyId}/projects";
            var body = new ProjectDTO
            {
                Name = $"Project {new Random().NextDouble()}"
            };
            var response = await client.PostAsync(uri, body.ToStringContent());
            return await response.Content.Parse<ProjectDTO>();
        }

        public static async Task<StateDTO> CreateProjectState(this HttpClient client, ObjectId projectId)
        {
            var uri = $"api/projects/{projectId}/states";
            var body = new StateDTO
            {
                Name = $"Project {new Random().NextDouble()}",
                Phase = "Phase 1"
            };
            var response = await client.PostAsync(uri, body.ToStringContent());
            return await response.Content.Parse<StateDTO>();
        }
    }
    
    */
}