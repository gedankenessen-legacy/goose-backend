using Goose.API.Repositories;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Auth;
using Goose.Domain.Models.Projects;
using Goose.Domain.Models.Issues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Goose.API;
using Microsoft.Extensions.DependencyInjection;
using Goose.Domain.Models.Identity;
using System.Net.Http.Headers;
using MongoDB.Bson;

namespace Goose.Tests.Application.IntegrationTests
{
    /// Dieser Typ wird verwendet, um mehrere Testdocumente zu speichern
    using TestDocuments = Dictionary<int, ObjectId>;

    //Implemented as Singelton
    public sealed class TestHelper
    {
        private static readonly TestHelper _instance = new TestHelper();
        private const string FirmenName = "GooseTestFirma";
        private const string ProjektName = "GooseTestProject";
        private const string TicketName = "GooseTestIssue";

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static TestHelper()
        {
        }

        private readonly ICompanyRepository _companyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IIssueRepository _issueRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IProjectRepository _projectRepository;

        // Hier werden alle TestIssues gespeichert, die sich in der DB befinden.
        private readonly TestDocuments _testIssues = new();

        private TestHelper()
        {
            var factory = new WebApplicationFactory<Startup>();
            var scopeFactory = factory.Server.Services.GetService<IServiceScopeFactory>();

            using var scope = scopeFactory.CreateScope();
            _companyRepository = scope.ServiceProvider.GetService<ICompanyRepository>();
            _userRepository = scope.ServiceProvider.GetService<IUserRepository>();
            _projectRepository = scope.ServiceProvider.GetService<IProjectRepository>();
            _roleRepository = scope.ServiceProvider.GetService<IRoleRepository>();
            _issueRepository = scope.ServiceProvider.GetService<IIssueRepository>();
        }

        public static TestHelper Instance
        {
            get
            {
                return _instance;
            }
        }

        #region Clearing
        public async Task ClearCompany()
        {
            var company = (await _companyRepository.FilterByAsync(x => x.Name.Equals(FirmenName))).FirstOrDefault();

            if (company is not null)
            {
                foreach (var user in company.Users)
                    await _userRepository.DeleteAsync(user.UserId);
                await _companyRepository.DeleteAsync(company.Id);
            }
        }

        public async Task ClearProject()
        {
            var project = (await _projectRepository.FilterByAsync(x => x.ProjectDetail.Name.Equals(ProjektName))).FirstOrDefault();
            if (project is not null)
                await _projectRepository.DeleteAsync(project.Id);
        }

        public async Task ClearIssue()
        {
            foreach (var issueId in _testIssues.Values)
            {
                await _issueRepository.DeleteAsync(issueId);
            }

            _testIssues.Clear();
        }

        public async Task ClearAll()
        {
            await ClearIssue();
            await ClearProject();
            await ClearCompany();
        }
        #endregion

        #region Generating
        public async Task<SignInResponse> GenerateCompany(HttpClient client)
        {
            var uri = "/api/auth/signUp";
            SignUpRequest signUpRequest = new SignUpRequest() { Firstname = "Goose", Lastname = "Project", CompanyName = FirmenName, Password = "Test12345" };
            var response = await client.PostAsync(uri, signUpRequest.ToStringContent());
            return await response.Content.Parse<SignInResponse>();
        }

        public async Task GenerateProject(HttpClient client)
        {
            var company = (await _companyRepository.FilterByAsync(x => x.Name.Equals(FirmenName))).FirstOrDefault();
            var uri = $"api/companies/{company.Id}/projects";
            var newProject = new ProjectDTO() { Name = ProjektName };
            await client.PostAsync(uri, newProject.ToStringContent());
        }

        public async Task AddUserToProject(HttpClient client, string roleName)
        {
            // add user to company
            var user = await GetUser();
            var project = await GetProject();
            var role = (await _roleRepository.FilterByAsync(x => x.Name == roleName)).Single();
            var uri = $"api/projects/{project.Id}/users/{user.Id}";
            var addRequest = new PropertyUserDTO()
            {
                User = new UserDTO(user),
                Roles = new List<RoleDTO>()
                {
                    new RoleDTO(role),
                }
            };
            await client.PutAsync(uri, addRequest.ToStringContent());
        }

        public async Task GenerateIssue(HttpClient httpClient, int index = 0)
        {
            if (_testIssues.ContainsKey(index))
            {
                throw new Exception("Index existiert bereits");
            }

            var project = await GetProject();
            var user = await GetUser();

            var uri = $"api/projects/{project.Id}/issues/";

            var issue = new IssueDTO
            {
                Author = new UserDTO(user),
                Client = new UserDTO(user),
                Project = new ProjectDTO(project),
                State = await GetStateByName(httpClient, project.Id, State.NegotiationState),
                IssueDetail = new IssueDetail
                {
                    Name = TicketName,
                    Type = Issue.TypeFeature,
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
            var postResult = await httpClient.PostAsync(uri, issue.ToStringContent());
            var result = await postResult.Content.Parse<IssueDTO>();

            _testIssues[index] = result.Id;
        }

        /// <summary>
        /// Generates a Test Company, User, Project and Issue.
        /// These objects can be retrieved from the DB via the Get???() methods.
        /// It also adds an authorisation header to the provided client.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public async Task GenerateAll(HttpClient client)
        {
            var signInResult = await GenerateCompany(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", signInResult.Token);
            await GenerateProject(client);
            await GenerateIssue(client);
        }
        #endregion


        #region Getting
        private async Task<IList<StateDTO>> GetStateList(HttpClient client, ObjectId projectId)
        {
            var uri = $"api/projects/{projectId}/states";
            var responce = await client.GetAsync(uri);
            return await responce.Content.Parse<IList<StateDTO>>();
        }

        public async Task<StateDTO> GetStateByName(HttpClient client, ObjectId projectId, string name)
        {
            return (await GetStateList(client, projectId)).FirstOrDefault(x => x.Name.Equals(name));
        }

        public async Task<Domain.Models.Companies.Company> GetCompany()
        {
            var companies = await _companyRepository.FilterByAsync(x => x.Name == FirmenName);
            return companies.FirstOrDefault();
        }

        public async Task<Project> GetProject()
        {
            var projects = await _projectRepository.FilterByAsync(x => x.ProjectDetail.Name == ProjektName);
            return projects.FirstOrDefault();
        }

        public async Task<Issue> GetIssueAsync(int issueIndex = 0)
        {
            var issueId = _testIssues[issueIndex];
            var issues = await _issueRepository.FilterByAsync(x => x.Id == issueId);
            return issues.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves an issue through the Rest-api. Useful for testing the api from end to end
        /// </summary>
        /// <param name="_client"></param>
        /// <param name="issueIndex"></param>
        /// <returns></returns>
        public async Task<IssueDTODetailed> GetIssueThroughClientAsync(HttpClient _client, int issueIndex = 0)
        {
            var project = await GetProject();
            var issueId = _testIssues[issueIndex];
            var uri = $"api/projects/{project.Id}/issues/{issueId}?GetAll=true";

            var result = await _client.GetAsync(uri);
            return await result.Content.Parse<IssueDTODetailed>();
        }

        public async Task<User> GetUser()
        {
            var company = await GetCompany();
            var propertyUser = company.Users.First();

            var users = await _userRepository.FilterByAsync(x => x.Id.Equals(propertyUser.UserId));
            return users.FirstOrDefault();
        }
        #endregion
    }
}
