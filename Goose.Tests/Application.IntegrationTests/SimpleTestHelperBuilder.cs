using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Goose.API;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Auth;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Issues;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Goose.Tests.Application.IntegrationTests
{
    public class SimpleTestHelperBuilder
    {
        private SignUpRequest _signUpRequest = new SignUpRequest
        {
            Firstname = $"{new Random().NextDouble()}",
            Lastname = $"{new Random().NextDouble()}",
            Password = "Test123456!",
            CompanyName = $"{new Random().NextDouble()}"
        };

        private ProjectDTO _projectDto = new ProjectDTO
        {
            Name = $"{new Random().NextDouble()}"
        };

        protected IssueDTO _issueDto = new IssueDTO
        {
            Author = null,
            Client = null,
            Project = null,
            State = null,
            IssueDetail = new IssueDetail()
            {
                Name = $"{new Random().NextDouble()}",
                Type = Issue.TypeFeature,
                StartDate = null,
                EndDate = null,
                ExpectedTime = 0,
                Progress = 0,
                Description = null,
                Requirements = null,
                RequirementsAccepted = false,
                RequirementsSummaryCreated = false,
                RequirementsNeeded = true,
                Priority = 0,
                Visibility = false,
                RelevantDocuments = null
            }
        };


        public virtual async Task<SignInResponse> CreateCompanyAndLogin(HttpClient client, SimpleTestHelper helper)
        {
            var res = await helper.SignUp(_signUpRequest);
            var signIn = await res.Parse<SignInResponse>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", signIn.Token);
            return signIn;
        }

        public virtual async Task<ProjectDTO> CreateProject(HttpClient client, SimpleTestHelper helper)
        {
            var res = await helper.CreateProject(_projectDto);
            return await res.Parse<ProjectDTO>();
        }

        public virtual async Task AddUserToProject(HttpClient client, SimpleTestHelper helper)
        {
            await helper.Helper.AddUserToProject(helper.Project.Id, helper.User.Id, Role.ProjectLeaderRole);
        }

        public virtual IssueDTO GetIssueDTOCopy(HttpClient client, SimpleTestHelper helper)
        {
            _issueDto.Author = helper.User;
            _issueDto.Client = helper.User;
            _issueDto.Project = helper.Project;
            return _issueDto.Copy();
        }

        public virtual async Task<IssueDTO> CreateIssue(HttpClient client, SimpleTestHelper helper)
        {
            _issueDto.Author = helper.User;
            _issueDto.Client = helper.User;
            _issueDto.Project = helper.Project;
            return await helper.CreateIssue(GetIssueDTOCopy(client, helper)).Parse<IssueDTO>();
        }


        public virtual async Task<SimpleTestHelper> Build()
        {
            var factory = new WebApplicationFactory<Startup>();
            var client = factory.CreateClient();
            var helper = new SimpleTestHelper(factory, client);
            
            var signIn = CreateCompanyAndLogin(client, helper);
            if (signIn != null) helper.SignIn = await signIn;

            var project = CreateProject(client, helper);
            if (project != null) helper.Project = await project;

            var added = AddUserToProject(client, helper);
            if (added != null) await added;
            
            var issue = CreateIssue(client, helper);
            if (issue != null) helper.Issue = await issue;
            return helper;
        }
    }
}