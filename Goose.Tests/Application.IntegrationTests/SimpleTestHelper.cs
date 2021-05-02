using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Auth;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Issues;
using MongoDB.Bson;

namespace Goose.Tests.Application.IntegrationTests
{
    public class SimpleTestHelper : IDisposable
    {
        private readonly HttpClient _client;

        public readonly NewTestHelper Helper;

        public SimpleTestHelper(HttpClient client)
        {
            _client = client;
            Helper = new NewTestHelper(client);
        }

        public SignInResponse SignIn { get; set; }
        public CompanyDTO Company => SignIn.Companies[0];
        public UserDTO User => SignIn.User;
        public ProjectDTO Project { get; set; }
        public IssueDTO Issue { get; set; }

        public async Task<HttpResponseMessage> SignUp(SignUpRequest signUpRequest)
        {
            var res = await Helper.GenerateCompany();
            try
            {
                SignIn = await res.Parse<SignInResponse>();
            }
            catch (Exception)
            {
            }

            return res;
        }

        public async Task<HttpResponseMessage> CreateProject(ProjectDTO project)
        {
            var res = await Helper.GenerateProject(Company.Id, project);
            try
            {
                Project = await res.Parse<ProjectDTO>();
            }
            catch (Exception)
            {
            }

            return res;
        }

        public async Task<HttpResponseMessage> CreateIssue(IssueDTO issue)
        {
            var res = await Helper.GenerateIssue(Project, issue);
            try
            {
                Issue = await res.Parse<IssueDTO>();
            }
            catch (Exception)
            {
            }

            return res;
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

        public async Task<Issue> GetIssueAsync(ObjectId issueId)
        {
            return await Helper.GetIssueAsync(issueId);
        }


        public void Dispose()
        {
            _client?.Dispose();
            Helper?.Dispose();
        }
    }

    public class SimpleTestHelperBuilder
    {
        private readonly HttpClient _client;

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

        private IssueDTO _issueDto = new IssueDTO
        {
            Author = null,
            Client = null,
            Project = null,
            State = null,
            IssueDetail = new IssueDetail()
            {
                Name = $"{new Random().NextDouble()}",
                Type = Issue.TypeFeature,
                StartDate = default,
                EndDate = default,
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

        public SimpleTestHelperBuilder(HttpClient client)
        {
            _client = client;
        }

        public SimpleTestHelperBuilder SetIssue(IssueDTO issue)
        {
            _issueDto = issue;
            return this;
        }

        public SimpleTestHelperBuilder WithIssue(Action<IssueDTO> action)
        {
            action(_issueDto);
            return this;
        }

        public async Task<SimpleTestHelper> Build()
        {
            var simpleHelper = new SimpleTestHelper(_client);
            await simpleHelper.SignUp(_signUpRequest);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", simpleHelper.SignIn.Token);
            await simpleHelper.CreateProject(_projectDto);
            await simpleHelper.Helper.AddUserToProject(simpleHelper.Project.Id, simpleHelper.User.Id, Role.ProjectLeaderRole);


            _issueDto.Author = simpleHelper.User;
            _issueDto.Client = simpleHelper.User;
            _issueDto.Project = simpleHelper.Project;
            await simpleHelper.CreateIssue(_issueDto);

            return simpleHelper;
        }
    }
}