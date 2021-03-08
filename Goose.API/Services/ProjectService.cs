using Goose.API.Repositories;
using Goose.Domain.DTOs;
using Goose.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Services
{
    public interface IProjectService
    {
        Task<ProjectDTO> CreateNewProjectAsync(ProjectDTO requestedProject);
        Task<ProjectDTO> UpdateProject(string id, ProjectDTO projectDTO);
        IAsyncEnumerable<ProjectDTO> GetProjects();
        Task<ProjectDTO> GetProject(string id);
    }

    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public Task<ProjectDTO> CreateNewProjectAsync(ProjectDTO requestedProject)
        {
            throw new NotImplementedException();
        }

        public Task<ProjectDTO> GetProject(string id)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<ProjectDTO> GetProjects()
        {
            throw new NotImplementedException();
        }

        public Task<ProjectDTO> UpdateProject(string id, ProjectDTO projectDTO)
        {
            throw new NotImplementedException();
        }
    }
}
