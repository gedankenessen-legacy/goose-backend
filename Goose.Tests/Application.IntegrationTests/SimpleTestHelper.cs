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

        public async Task<StateDTO> GetState()
        {
            return await Helper.GetState(Issue);
        }

        public async Task<HttpResponseMessage> SignUp(SignUpRequest signUpRequest)
        {
            return await Helper.GenerateCompany();
        }

        public async Task<HttpResponseMessage> CreateProject(ProjectDTO project)
        {
            return await Helper.GenerateProject(Company.Id, project);
        }

        public async Task<HttpResponseMessage> CreateProject()
        {
            var copy = Project.Copy();
            copy.Id = ObjectId.Empty;
            return await CreateProject(copy);
        }

        /*
         * Returns project
         */
        public async Task<HttpResponseMessage> CreateProjectAndAddProjectUser(params Role[] roles)
        {
            var copy = Project.Copy();
            copy.Id = ObjectId.Empty;
            return await CreateProjectAndAddProjectUser(copy, SignIn.User.Id, roles);
        }

        public async Task<HttpResponseMessage> CreateProjectAndAddProjectUser(ProjectDTO project, ObjectId userId, params Role[] roles)
        {
            var res = CreateProject(project);
            var proj = await res.Parse<ProjectDTO>();
            await Helper.AddUserToProject(proj.Id, userId, roles);
            return await res;
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

        public async Task<IssueDTO> CreateChild()
        {
            if (Issue == default) throw new Exception("CreateIssue can only be called after CreateIssue(IssueDTO issue) was called first");
            var copy = Issue.Copy();
            copy.Id = ObjectId.Empty;
            return await CreateChild(copy);
        }

        public async Task<IssueDTO> CreateChild(IssueDTO issue)
        {
            var child = await CreateIssue(issue).Parse<IssueDTO>();
            await SetIssueChild(child.Id);
            return child;
        }

        public async Task<HttpResponseMessage> CreateState(StateDTO newState)
        {
            return await Helper.CreateStateInProject(Project.Id, newState: newState);
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

        public async Task<HttpResponseMessage> SetIssueChild(ObjectId childId)
        {
            return await Helper.SetParentIssue(Issue.Id, childId);
        }

        public async Task<HttpResponseMessage> SetPredecessor(ObjectId predecessorId)
        {
            return await Helper.SetPredecessor(predecessorId, Issue.Id);
        }

        public async Task<HttpResponseMessage> SetState(string stateName)
        {
            return await Helper.SetStateOfIssue(Issue, stateName);
        }

        public async Task AcceptSummary()
        {
            Issue.IssueDetail.ExpectedTime = 1.0;
            await Helper.AcceptSummary(Issue.Id);
        }


        public void Dispose()
        {
            _factory?.Dispose();
            client?.Dispose();
            Helper?.Dispose();
        }
    }
}