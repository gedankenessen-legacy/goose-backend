using Goose.API.Repositories;
using Goose.Domain.DTOs;
using Goose.Domain.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Utils;
using Goose.API.Utils.Exceptions;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Companies;
using Microsoft.AspNetCore.Authorization;
using Goose.API.Authorization.Requirements;
using Microsoft.AspNetCore.Http;
using Goose.API.Authorization;

namespace Goose.API.Services
{
    public interface IProjectUserService
    {
        Task<IList<PropertyUserDTO>> GetProjectUsers(ObjectId projectId);
        Task<PropertyUserDTO> GetProjectUser(ObjectId projectId, ObjectId userId);
        Task UpdateProjectUser(ObjectId projectId, ObjectId userId, PropertyUserDTO projectUserDTO);
        Task RemoveUserFromProject(ObjectId projectId, ObjectId userId);
    }

    public class ProjectUserService : IProjectUserService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectUserService(IProjectRepository projectRepository, IUserRepository userRepository, IRoleRepository roleRepository, IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor, ICompanyRepository companyRepository)
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _companyRepository = companyRepository;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PropertyUserDTO> GetProjectUser(ObjectId projectId, ObjectId userId)
        {
            var project = await _projectRepository.GetAsync(projectId);
            if (project == null)
            {
                // ungültige projectId
                throw new HttpStatusException(StatusCodes.Status404NotFound, "Project id does not exist");
            }

            var projectUser = project.Users.SingleOrDefault(x => x.UserId == userId);
            if (projectUser == null)
            {
                // user nicht Mitglied der Projects
                throw new HttpStatusException(StatusCodes.Status403Forbidden, "User is not a member of this project");
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
                throw new HttpStatusException(404, "Ungültige ProjectId");
            }

            var userIds = from projectUser in project.Users
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
            
            var userDTOs = from projectUser in project.Users
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
                throw new HttpStatusException(400, "User id does not match");
            }

            var roleIds = from role in projectUserDTO.Roles
                          select role.Id;

            var newProjectUser = new PropertyUser()
            {
                UserId = userId,
                RoleIds = roleIds.ToList(),
            };

            var existingProject = await _projectRepository.GetAsync(projectId);

            if (existingProject == null)
            {
                throw new HttpStatusException(404, "Invalid projectId");
            }

            Company company = await _companyRepository.GetAsync(existingProject.CompanyId);

            if (company is null)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "Company not found.");

            await AuthorizeUpdateAsync(company);

            var projectLeaderRole = Role.ProjectLeaderRole;

            if (projectLeaderRole != null && roleIds.Contains(projectLeaderRole.Id))
            {
                // Nutzer soll die Projektleiterrolle bekommen, in jedem Projekt
                // darf es aber nur einen ProjektLeiter geben

                var existingProjectLeader = from projectUser in existingProject.Users
                                            where projectUser.RoleIds.Contains(projectLeaderRole.Id)
                                            select projectUser;

                if (existingProjectLeader.Any())
                {
                    throw new HttpStatusException(403, "Cannot make two users a project Leader");
                }
            }

            existingProject.Users.ReplaceOrInsert(x => x.UserId == userId, newProjectUser);

            await _projectRepository.UpdateAsync(existingProject);
        }

        private async Task<bool> AuthorizeUpdateAsync(Company company)
        {
            // Dict with the requirement as key und the error message as value.
            Dictionary<IAuthorizationRequirement, string> requirementsWithErrors = new()
            {
                { CompanyRolesRequirement.CompanyOwner, "You need to be the owner of this company, in order to update the users" },
                { ProjectRolesRequirement.LeaderRequirement, "You need to be the Project leader, in order to update the users"}
            };

            // validate requirements with the appropriate handlers.
            (await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, company, requirementsWithErrors.Keys)).ThrowErrorIfAllFailed(requirementsWithErrors);

            return true;
        }

        public async Task RemoveUserFromProject(ObjectId projectId, ObjectId userId)
        {
            var existingProject = await _projectRepository.GetAsync(projectId);

            if (existingProject == null)
            {
                throw new HttpStatusException(404, "Invalid projectId");
            }

            var success = existingProject.Users.Remove(x => x.UserId == userId);

            if (!success)
            {
                throw new HttpStatusException(400, "User not part of this project");
            }

            await _projectRepository.UpdateAsync(existingProject);
        }

    }
}
