using Goose.API.Repositories;
using Goose.Domain.DTOs;
using Goose.Domain.Models;
using Goose.Domain.Models.projects;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Services
{
    public interface IProjectService
    {
        Task<ProjectDTO> CreateNewProjectAsync(ObjectId companyId, ProjectDTO requestedProject);
        Task UpdateProject(ObjectId projectId, ProjectDTO projectDTO);
        Task<IList<ProjectDTO>> GetProjects();
        Task<ProjectDTO> GetProject(ObjectId projectId);
    }

    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<ProjectDTO> CreateNewProjectAsync(ObjectId companyId, ProjectDTO requestedProject)
        {
            var newProject = new Project()
            {
                Id = new ObjectId(),
                CompanyId = companyId,
                ProjectDetail = new ProjectDetail()
                {
                    Name = requestedProject.Name,
                }
            };

            await _projectRepository.CreateAsync(newProject);

            return new ProjectDTO(newProject);
        }

        public async Task<ProjectDTO> GetProject(ObjectId projectId)
        {
            var project = await _projectRepository.GetAsync(projectId);
            return new ProjectDTO(project);
        }

        public async Task<IList<ProjectDTO>> GetProjects()
        {
            var projects = await _projectRepository.GetAsync();

            var projectDTOs = from project in projects
                              select new ProjectDTO(project);

            return projectDTOs.ToList();
        }

        public async Task UpdateProject(ObjectId projectId, ProjectDTO projectDTO)
        {
            if (projectDTO.Id != projectId.ToString())
            {
                throw new Exception("Cannot Update: Project ID does not match");
            }

            await _projectRepository.UpdateProject(projectId, projectDTO.Name);
        }
    }
}
