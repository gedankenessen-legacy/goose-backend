using Goose.API.Authorization.Requirements;
using Goose.API.Repositories;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs;
using Goose.Domain.Models.Companies;
using Goose.Domain.Models.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Goose.Domain.Models;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Goose.API.Authorization;
using Goose.API.Utils.Authentication;
using Goose.Domain.Models.Identity;

namespace Goose.API.Services
{
    public interface IProjectService
    {
        Task<ProjectDTO> CreateProjectAsync(ObjectId companyId, ProjectDTO requestedProject);
        Task UpdateProject(ObjectId projectId, ProjectDTO projectDTO);
        Task<IList<ProjectDTO>> GetProjects(ObjectId companyId);
        Task<ProjectDTO> GetProject(ObjectId projectId);
    }

    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectService(
            IProjectRepository projectRepository,
            ICompanyRepository companyRepository,
            IRoleRepository roleRepository,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _projectRepository = projectRepository;
            _companyRepository = companyRepository;
            _roleRepository = roleRepository;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ProjectDTO> CreateProjectAsync(ObjectId companyId, ProjectDTO requestedProject)
        {
            // is client allowed to create a project? Clients needs to have the role "companyOwner" in the provided <companyId>.
            Company company = await _companyRepository.GetAsync(companyId);

            if (company is null)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "Company not found.");

            await AuthorizeCreationAsync(company);

            var newProject = new Project()
            {
                Id = ObjectId.GenerateNewId(),
                CompanyId = companyId,
                ProjectDetail = new ProjectDetail()
                {
                    Name = requestedProject.Name,
                },
                States = GetDefaultStates()
            };

            await _projectRepository.CreateAsync(newProject);

            return new ProjectDTO(newProject);
        }

        private async Task<bool> AuthorizeCreationAsync(Company company)
        {
            // Dict with the requirement as key und the error message as value.
            Dictionary<IAuthorizationRequirement, string> requirementsWithErrors = new()
            {
                { CompanyRolesRequirement.CompanyOwner, "You need to be the owner of this company, in order to create a project."},
                //{ new ProjectHasClientRequirement(), "Your company is missing a client, in order to create a project." }
            };

            // validate requirements with the appropriate handlers.
            (await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, company, requirementsWithErrors.Keys)).ThrowErrorForFailedRequirements(requirementsWithErrors);

            return true;
        }

        private IList<State> GetDefaultStates()
        {
            State CreateDefaultState(string name, string phase)
            {
                return new State()
                {
                    Id = ObjectId.GenerateNewId(),
                    Name = name,
                    Phase = phase,
                    UserGenerated = false,
                };
            }

            var states = new List<State>()
            {
                CreateDefaultState(State.CheckingState, State.NegotiationPhase),
                CreateDefaultState(State.NegotiationState, State.NegotiationPhase),

                CreateDefaultState(State.BlockedState, State.ProcessingPhase),
                CreateDefaultState(State.WaitingState, State.ProcessingPhase),
                CreateDefaultState(State.ProcessingState, State.ProcessingPhase),
                CreateDefaultState(State.ReviewState, State.ProcessingPhase),

                CreateDefaultState(State.CompletedState, State.ConclusionPhase),
                CreateDefaultState(State.CancelledState, State.ConclusionPhase),
                CreateDefaultState(State.ArchivedState, State.ConclusionPhase),
            };

            return states;
        }

        public async Task<ProjectDTO> GetProject(ObjectId projectId)
        {
            var project = await _projectRepository.GetAsync(projectId);

            return new ProjectDTO(project);
        }

        public async Task<IList<ProjectDTO>> GetProjects(ObjectId companyId)
        {
            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            var company = await _companyRepository.GetAsync(companyId);

            if (company == null)
            {
                throw new HttpStatusException(StatusCodes.Status404NotFound, "Invalid CompanyId");
            }

            var companyUser = company.Users.SingleOrDefault(x => x.UserId == userId);
            if (companyUser == null)
            {
                throw new HttpStatusException(StatusCodes.Status403Forbidden, "You are no member of this company.");
            }

            var companyRole = (await _roleRepository.FilterByAsync(x => x.Name == Role.CompanyRole.Name)).Single();

            var projects = await _projectRepository.FilterByAsync(x => x.CompanyId == companyId);

            IEnumerable<ProjectDTO> projectDTOs;
            if (companyUser.RoleIds.Any(x => x == companyRole.Id))
            {
                // Die Firma darf alle Projekt sehen
                projectDTOs = from project in projects
                              select new ProjectDTO(project);
            }
            else
            {
                // Ansonsten (Mitarbeiter & Kunden) dürfen nur Projekte sehen, in denen sie auch eine Rolle haben
                projectDTOs = from project in projects
                              where project.Users.Any(x => x.UserId == userId)
                              select new ProjectDTO(project);
            }

            return projectDTOs.ToList();
        }

        public async Task UpdateProject(ObjectId projectId, ProjectDTO projectDTO)
        {
            await _projectRepository.UpdateProject(projectId, projectDTO.Name);
        }
    }
}
