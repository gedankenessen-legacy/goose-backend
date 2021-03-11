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
        Task<ProjectDTO> CreateNewProjectAsync(ProjectDTO requestedProject);
        Task UpdateProject(ObjectId id, ProjectDTO projectDTO);
        Task<IList<ProjectDTO>> GetProjects();
        Task<ProjectDTO> GetProject(ObjectId id);
    }

    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<ProjectDTO> CreateNewProjectAsync(ProjectDTO requestedProject)
        {
            var newProject = new Project()
            {
                // TODO do we get this is or do we need to generate one?
                Id = new ObjectId(requestedProject.Id),
                CompanyId = new ObjectId(requestedProject.CompanyId),
                ProjectDetail = new ProjectDetail()
                {
                    Name = requestedProject.Name,
                }
            };

            await _projectRepository.CreateAsync(newProject);

            return new ProjectDTO(newProject);
        }

        public async Task<ProjectDTO> GetProject(ObjectId id)
        {
            var project = await _projectRepository.GetAsync(id);
            return new ProjectDTO(project);
        }

        public async Task<IList<ProjectDTO>> GetProjects()
        {
            var projects = await _projectRepository.GetAsync();

            var projectDTOs = from project in projects
                              select new ProjectDTO(project);

            return projectDTOs.ToList();
        }

        public async Task UpdateProject(ObjectId id, ProjectDTO projectDTO)
        {
            if (projectDTO.Id != id.ToString())
            {
                // TODO what to do in this case?
                throw new Exception("Cannot Update: Project ID does not match");
            }

            await _projectRepository.UpdateProject(id, projectDTO.Name, new ObjectId(projectDTO.CompanyId));
        }
    }
}
