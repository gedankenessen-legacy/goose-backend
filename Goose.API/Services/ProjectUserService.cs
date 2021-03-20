using Goose.API.Repositories;
using Goose.Domain.DTOs;
using Goose.Domain.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Services
{
    public interface IProjectUserService
    {
        Task<IList<PropertyUserDTO>> GetProjectUsers(ObjectId projectId);
        Task<PropertyUserDTO> GetProjectUser(ObjectId projectId, ObjectId userId);
        Task UpdateProjectUser(ObjectId projectId, ObjectId userId, PropertyUserDTO projectUserDTO);
    }

    public class ProjectUserService : IProjectUserService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public ProjectUserService(IProjectRepository projectRepository, IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        public async Task<PropertyUserDTO> GetProjectUser(ObjectId projectId, ObjectId userId)
        {
            var project = await _projectRepository.GetAsync(projectId);
            if (project == null)
            {
                // ungültige projectId
                return null;
            }

            var projectUser = project.ProjectUsers.SingleOrDefault(x => x.UserId == userId);
            
            if (projectUser == null)
            {
                // user nicht Mitglied der Projects
                return null;
            }

            var user = await _userRepository.GetAsync(userId);
            var roles = await _roleRepository.GetAsync(projectUser.RoleIds);
            var roleDTOs = from role in roles
                           select new RoleDTO(role);

            return new PropertyUserDTO()
            {
                User = new UserDTO(user),
                Roles = roleDTOs.ToList(),
            };
        }

        public async Task<IList<PropertyUserDTO>> GetProjectUsers(ObjectId projectId)
        {
            var project = await _projectRepository.GetAsync(projectId);
            if (project == null)
            {
                // ungültige projectId
                return new List<PropertyUserDTO>();
            }

            var userIds = from projectUser in project.ProjectUsers
                          select projectUser.UserId;
            var users = await _userRepository.GetAsync(userIds);

            // wir holen einfach immer alle rollen aus der Datenbank
            var roles = await _roleRepository.GetAsync();

            // Hier wird eine innere Funktion verwendet, damit die Rollen einfach nachschlagen werden können.
            // Dazu wird ein Dictionary aufgebaut, indem die roleDTOs zu einer bestimmten Id bereit gehalten werden
            var rolesDict = roles.ToDictionary(x => x.Id, x => new RoleDTO(x));
            IList<RoleDTO> GetRoleDTOs(PropertyUser projectUser)
            {
                var result = from roleId in projectUser.RoleIds
                             select rolesDict[roleId];

                return result.ToList();
            }
            
            var userDTOs = from projectUser in project.ProjectUsers
                           join user in users on projectUser.UserId equals user.Id
                           select new PropertyUserDTO()
                           {
                               User = new UserDTO(user),
                               Roles = GetRoleDTOs(projectUser),
                           };

            return userDTOs.ToList();
        }

        public async Task UpdateProjectUser(ObjectId projectId, ObjectId userId, PropertyUserDTO projectUserDTO)
        {
            if (userId != projectUserDTO.User.Id)
            {
                throw new Exception("User id does not match");
            }

            var roleIds = from role in projectUserDTO.Roles
                          select role.Id;

            var propertyUser = new PropertyUser()
            {
                UserId = userId,
                RoleIds = roleIds.ToList(),
            };

            var existingProject = await _projectRepository.GetAsync(projectId);

            if (existingProject == null)
            {
                throw new Exception("Invalid projectId");
            }

            // TODO insert / update
        }
    }
}
