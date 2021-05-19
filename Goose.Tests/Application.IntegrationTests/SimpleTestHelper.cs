using System;
using System.Net.Http;
using System.Threading.Tasks;
using Goose.API;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Auth;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Issues;
using Microsoft.AspNetCore.Mvc.Testing;
using MongoDB.Bson;

namespace Goose.Tests.Application.IntegrationTests
{
    public class SimpleTestHelper : IDisposable
    {
        private readonly WebApplicationFactory<Startup> _factory;
        public readonly HttpClient client;

        public readonly NewTestHelper Helper;

        public SimpleTestHelper(WebApplicationFactory<Startup> factory, HttpClient client)
        {
            _factory = factory;
            this.client = client;
            Helper = new NewTestHelper(client);
        }

        public SignInResponse SignIn { get; set; }
        public CompanyDTO Company => SignIn.Companies[0];
        public UserDTO User => SignIn.User;
        public ProjectDTO Project { get; set; }
        public IssueDTO Issue { get; set; }

        public async Task<HttpResponseMessage> SignUp(SignUpRequest signUpRequest)
        {
            return await Helper.GenerateCompany();
        }

        public async Task<HttpResponseMessage> CreateProject(ProjectDTO project)
        {
            return await Helper.GenerateProject(Company.Id, project);
        }

        public async Task<HttpResponseMessage> CreateIssue(IssueDTO issue)
        {
            return await Helper.GenerateIssue(Project, issue);
        }

        /**
         * Creates a new issue with the same properties as the default issue (Generated in CreateIssue(IssueDTO issue))
         */
        public async Task<HttpResponseMessage> CreateIssue()
        {
            if (Issue == default) throw new Exception("CreateIssue can only be called after CreateIssue(IssueDTO issue) was called first");
            var copy = Issue.Copy();
            copy.Id = ObjectId.Empty;

            return await Helper.GenerateIssue(Project, copy);
        }

        public async Task<UserDTO> GenerateUserAndSetToProject(params Role[] roles)
        {
            client.Auth(SignIn);
            return await Helper.GenerateUserAndSetToProject(Company.Id, Project.Id, roles);
        }

        public async Task<Issue> GetIssueAsync(ObjectId issueId)
        {
            return await Helper.GetIssueAsync(issueId);
        }


        public void Dispose()
        {
            _factory?.Dispose();
            client?.Dispose();
            Helper?.Dispose();
        }
    }
}