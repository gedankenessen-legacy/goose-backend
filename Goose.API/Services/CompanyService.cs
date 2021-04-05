using Goose.API.Repositories;
using Goose.API.Utils.Authentication;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs;
using Goose.Domain.Models;
using Goose.Domain.Models.Companies;
using Goose.Domain.Models.Identity;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Services
{
    public interface ICompanyService
    {
        public Task<CompanyDTO> GetCompanyAsync(string companyId);
        public Task<IList<CompanyDTO>> GetCompaniesAsync(ObjectId? userId = null);
        public Task<CompanyDTO> CreateCompanyAsync(string companyName, ObjectId creatorUserId);
        public Task<CompanyDTO> UpdateCompanyAsync(string id, CompanyDTO company);
        
        public Task<bool> CompanyNameAvailableAsync(string companyName);
        public Task<bool> UserHasRoleInCompany(ObjectId userId, ObjectId companyId, params string[] roles);

        public Task<IList<PropertyUserDTO>> GetCompanyUsersAsync(string companyId);
        public Task<PropertyUserDTO> GetCompanyUserAsync(string companyId, string userId);
    }

    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IRoleRepository _roleRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompanyService(ICompanyRepository companyRepository, IUserService userService, IRoleService roleService, IRoleRepository roleRepository, IHttpContextAccessor httpContextAccessor)
        {
            _companyRepository = companyRepository;
            _userService = userService;
            _roleService = roleService;
            _roleRepository = roleRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CompanyDTO> CreateCompanyAsync(string companyName, ObjectId creatorUserId)
        {
            if (ObjectId.TryParse(creatorUserId.ToString(), out _) is false)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "The user id is not valid.");

            if (await CompanyNameAvailableAsync(companyName) is false)
                throw new HttpStatusException(StatusCodes.Status409Conflict, "A company with this name is already existing.");

            var role = (await _roleRepository.FilterByAsync(x => x.Name.Equals(Role.CompanyRole))).FirstOrDefault();

            if (role is null)
                role = await _roleService.CreateRoleAsync(new Role() { Name = "Firma" });

            var newCompany = new Company()
            {
                Name = companyName,
                ProjectIds = new List<ObjectId>(),
                Users = new List<PropertyUser>() 
                { 
                    new PropertyUser() 
                    { 
                        RoleIds = new List<ObjectId>() 
                        { 
                            role.Id 
                        },
                        UserId = creatorUserId
                    } 
                }
            };

            await _companyRepository.CreateAsync(newCompany);

            return new CompanyDTO() { Id = newCompany.Id, Name = newCompany.Name };
        }

        public async Task<IList<CompanyDTO>> GetCompaniesAsync(ObjectId? userId = null)
        {
            if (userId is null)
                userId = _httpContextAccessor.HttpContext.User.GetUserId();

            //TODO: maybe move to repo.
            var companies = await _companyRepository.FilterByAsync(cmp => cmp.Users.Any(pu => pu.UserId.Equals(userId)));

            if (companies is null)
                throw new Exception("Something went wrong");

            var companyDTOs = new List<CompanyDTO>();

            foreach (var company in companies)
            {
                var companyDTO = (CompanyDTO)company;
                companyDTOs.Add(companyDTO);
            }

            return companyDTOs;
        }

        public async Task<CompanyDTO> GetCompanyAsync(string companyId)
        {
            var company = await _companyRepository.GetCompanyByIdAsync(companyId);

            return (CompanyDTO)company;
        }

        public async Task<CompanyDTO> UpdateCompanyAsync(string id, CompanyDTO company)
        {
            if (company is null)
                throw new Exception("something went wronge");

            if (!id.Equals(company.Id))
                throw new Exception();

            var companyToUpdate = await _companyRepository.GetCompanyByIdAsync(id);

            if (companyToUpdate is null)
                throw new Exception();

            companyToUpdate.Name = company.Name;

            await _companyRepository.UpdateAsync(companyToUpdate);

            return (CompanyDTO)companyToUpdate;
        }

        public async Task<IList<PropertyUserDTO>> GetCompanyUsersAsync(string companyId)
        {
            var company = await _companyRepository.GetCompanyByIdAsync(companyId);

            var userList = await _userService.GetUsersAsync();

            var roleList = await _roleService.GetRolesAsync();

            IList<PropertyUserDTO> propertyUserList = new List<PropertyUserDTO>();

            foreach (var propertyUser in company.Users)
            {
                var user = userList.FirstOrDefault(x => x.Id.Equals(propertyUser.UserId));

                IList<RoleDTO> roles = new List<RoleDTO>();

                foreach (var roleId in propertyUser.RoleIds)
                {
                    var role = roleList.FirstOrDefault(x => x.Id.Equals(roleId));

                    if (role is not null)
                        roles.Add(new RoleDTO(role));
                }

                propertyUserList.Add(new PropertyUserDTO() { User = user, Roles = roles });
            }

            return propertyUserList;
        }

        public async Task<PropertyUserDTO> GetCompanyUserAsync(string companyId, string userId)
        {
            var company = await _companyRepository.GetCompanyByIdAsync(companyId);

            if (company is null)
                throw new Exception("No Company with this Id exists");

            var propertyUser = company.Users.FirstOrDefault(x => x.UserId.Equals(userId.ToObjectId()));

            if (propertyUser is null)
                throw new Exception("There is no User with this ID");

            var user = await _userService.GetUser(propertyUser.UserId);

            if (user is null)
                throw new Exception("There is no User with this ID");

            IList<RoleDTO> roles = new List<RoleDTO>();

            var roleList = await _roleService.GetRolesAsync();

            foreach (var roleId in propertyUser.RoleIds)
            {
                var role = roleList.FirstOrDefault(x => x.Equals(roleId));

                if (role is not null)
                    roles.Add(new RoleDTO(role));
            }

            return new PropertyUserDTO() { User = user, Roles = roles };
        }

        public async Task<bool> CompanyNameAvailableAsync(string companyName)
        {
            var result = await _companyRepository.FilterByAsync(c => c.Name.ToLower() == companyName.ToLower());

            return result is null || !result.Any();
        }

        public async Task<bool> UserHasRoleInCompany(ObjectId userId, ObjectId companyId, params string[] roles)
        {
            Company company = await _companyRepository.GetAsync(companyId);

            if (company is null) throw new HttpStatusException(StatusCodes.Status400BadRequest, "Company not found.");

            PropertyUser companyUser = company.Users.Where(user => user.UserId.Equals(userId)).FirstOrDefault();

            if (companyUser is null) throw new HttpStatusException(StatusCodes.Status400BadRequest, "Error determin company roles for user.");

            foreach (var requestedRoleName in roles)
            {
                var requestedRole = await _roleRepository.GetFirstRoleByNameAsync(requestedRoleName);

                if (requestedRole is null)
                    throw new HttpStatusException(StatusCodes.Status400BadRequest, $"No role found with name: {requestedRoleName}");

                if (companyUser.RoleIds.Contains(requestedRole.Id))
                    return true;
            }

            return false;
        }
    }
}
