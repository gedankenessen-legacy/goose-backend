using Goose.API.Repositories;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Auth;
using Goose.Domain.Models.Projects;
using Goose.Domain.Models.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Tests.Application.IntegrationTests
{
    //Implemented as Singelton
    public sealed class TestHelper
    {
        private static readonly TestHelper instance = new TestHelper();
        public const string FirmenName = "GooseTestFirma";
        public const string ProjektName = "GooseTestProject";
        public const string TicketName = "GooseTestIssue";

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static TestHelper()
        {
        }

        private TestHelper()
        {
        }

        public static TestHelper Instance
        {
            get
            {
                return instance;
            }
        }

        public async Task ClearCompany(ICompanyRepository companyRepository, IUserRepository userRepository)
        {
            var company = (await companyRepository.FilterByAsync(x => x.Name.Equals(FirmenName))).FirstOrDefault();

            if (company is not null)
            {
                foreach (var user in company.Users)
                    await userRepository.DeleteAsync(user.UserId);
                await companyRepository.DeleteAsync(company.Id);
            }
        }

        public async Task ClearProject(IProjectRepository projectRepository)
        {
            var project = (await projectRepository.FilterByAsync(x => x.ProjectDetail.Name.Equals(ProjektName))).FirstOrDefault();
            if (project is not null)
                await projectRepository.DeleteAsync(project.Id);
        }

        public async Task ClearIssue(IIssueRepository issueRepository)
        {
            var issue = (await issueRepository.FilterByAsync(x => x.IssueDetail.Name.Equals(TicketName))).FirstOrDefault();
            if (issue is not null)
                await issueRepository.DeleteAsync(issue.Id);
        }

        public async Task<SignInResponse> GenerateCompany(HttpClient client)
        {
            var uri = "/api/auth/signUp";
            SignUpRequest signUpRequest = new SignUpRequest() { Firstname = "Goose", Lastname = "Project", CompanyName = FirmenName, Password = "Test12345" };
            var response = await client.PostAsync(uri, signUpRequest.ToStringContent());
            return await response.Content.Parse<SignInResponse>();
        }

        public async Task GenerateProject(HttpClient client, ICompanyRepository companyRepository)
        {
            var company = (await companyRepository.FilterByAsync(x => x.Name.Equals(FirmenName))).FirstOrDefault();
            var uri = $"api/companies/{company.Id}/projects";
            var newProject = new ProjectDTO() { Name = ProjektName };
            await client.PostAsync(uri, newProject.ToStringContent());
        }

        public async Task GenerateIssue(HttpClient client, ICompanyRepository companyRepository, IProjectRepository projectRepository, IUserRepository userRepository)
        {
            var company = (await companyRepository.FilterByAsync(x => x.Name.Equals(FirmenName))).FirstOrDefault();
            var project = (await projectRepository.FilterByAsync(x => x.ProjectDetail.Name.Equals(ProjektName))).FirstOrDefault();

            var propertyUser = company.Users.FirstOrDefault(x => x != null);
            var user = (await userRepository.FilterByAsync(x => x.Id.Equals(propertyUser.UserId))).FirstOrDefault();

            var uri = $"api/projects/{project.Id}/issues/";

            var issue = new IssueDTO
            {
                Author = new UserDTO(user),
                Client = new UserDTO(user),
                Project = new ProjectDTO(project),
                State = await GetStateByName(client, project.Id.ToString(), State.NegotiationState),
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
            var res = await client.PostAsync(uri, issue.ToStringContent());
            var dto = await res.Content.ReadAsStringAsync();
            return;
        }

        private async Task<IList<StateDTO>> GetStateList(HttpClient client, string projectId)
        {
            var uri = $"api/projects/{projectId}/states";
            var responce = await client.GetAsync(uri);
            return await responce.Content.Parse<IList<StateDTO>>();
        }

        public async Task<StateDTO> GetStateByName(HttpClient client, string projectId, string name)
        {
            return (await GetStateList(client, projectId)).FirstOrDefault(x => x.Name.Equals(name));
        }
    }
}
