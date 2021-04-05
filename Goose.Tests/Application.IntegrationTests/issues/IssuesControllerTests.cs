using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Goose.API;
using Goose.API.Utils;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.issues;
using Goose.Domain.Models.companies;
using Microsoft.AspNetCore.Mvc.Testing;
using MongoDB.Bson;
using Newtonsoft.Json;
using NUnit.Framework;
using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    /**
     * The issue endpoint is dependant of the State, Company, project and User controller. Tests may fail if those weren't tested first.
     */
    [TestFixture]
    public class IssuesControllerTests
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;

        private CompanyDTO _company { get; set; }
        private ProjectDTO _project { get; set; }
        private StateDTO _state { get; set; }

        [SetUp]
        public async Task Setup()
        {
            _factory = new WebApplicationFactory<Startup>();
            _client = _factory.CreateClient();

            _company = await _client.CreateCompany();
            _project = await _client.CreateProject(_company.Id);
            _state = await _client.CreateProjectState(_project.Id);
        }

        [Test]
        public async Task GetEmptyListOfIssuesFromProject()
        {
            var uri = $"api/projects/{_project.Id}/issues";
            var response = await _client.GetAsync(uri);
            var content = await response.Content.Parse<List<IssueDTODetailed>>();
            Assert.IsEmpty(content);
        }
    }


    static class IssueControllerTestsUtil
    {
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
}