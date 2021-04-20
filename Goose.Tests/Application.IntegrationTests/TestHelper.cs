﻿using Goose.API.Repositories;
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

namespace Goose.Tests.Application.IntegrationTests
{
    //Implemented as Singelton
    public sealed class TestHelper
    {
        private static readonly TestHelper instance = new TestHelper();
        private const string FirmenName = "GooseTestFirma";
        private const string ProjektName = "GooseTestProject";
        private const string TicketName = "GooseTestIssue";

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static TestHelper()
        {
        }

        private ICompanyRepository _companyRepository;
        private IUserRepository _userRepository;
        private IIssueRepository _issueRepository;
        private IProjectRepository _projectRepository;

        private TestHelper()
        {
            var factory = new WebApplicationFactory<Startup>();
            var client = factory.CreateClient();
            var scopeFactory = factory.Server.Services.GetService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope())
            {
                _companyRepository = scope.ServiceProvider.GetService<ICompanyRepository>();
                _userRepository = scope.ServiceProvider.GetService<IUserRepository>();
                _projectRepository = scope.ServiceProvider.GetService<IProjectRepository>();
                _issueRepository = scope.ServiceProvider.GetService<IIssueRepository>();
            }
        }

        public static TestHelper Instance
        {
            get
            {
                return instance;
            }
        }

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
            var issue = (await _issueRepository.FilterByAsync(x => x.IssueDetail.Name.Equals(TicketName))).FirstOrDefault();
            if (issue is not null)
                await _issueRepository.DeleteAsync(issue.Id);
        }

        public async Task ClearAll()
        {
            await ClearIssue();
            await ClearProject();
            await ClearCompany();
        }

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

        public async Task GenerateIssue(HttpClient client)
        {
            var project = await GetProject();
            var user = await GetUser();

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
            await client.PostAsync(uri, issue.ToStringContent());
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

        public async Task<Domain.Models.Companies.Company> GetCompany()
        {
            var companies = await _companyRepository.FilterByAsync(x => x.Name.Equals(FirmenName));
            return companies.FirstOrDefault();
        }

        public async Task<Project> GetProject()
        {
            var projects = await _projectRepository.FilterByAsync(x => x.ProjectDetail.Name.Equals(ProjektName));
            return projects.FirstOrDefault();
        }

        public async Task<Issue> GetIssueAsync()
        {
            var issues = await _issueRepository.FilterByAsync(x => x.IssueDetail.Name == TestHelper.TicketName);
            return issues.FirstOrDefault();
        }

        public async Task<User> GetUser()
        {
            var company = await GetCompany();
            var propertyUser = company.Users.First();

            var users = await _userRepository.FilterByAsync(x => x.Id.Equals(propertyUser.UserId));
            return users.FirstOrDefault();
        }
    }
}
