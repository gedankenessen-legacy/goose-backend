using Goose.API.Repositories;
using Goose.Domain.DTOs;
using Goose.Domain.Models;
using Goose.Domain.Models.Companies;
using Goose.Domain.Models.Identity;
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
        public Task<IList<CompanyDTO>> GetCompaniesAsync();
        public Task<CompanyDTO> CreateCompanyAsync(CompanyLogin companyLogin);
        public Task<CompanyDTO> UpdateCompanyAsync(string id, CompanyDTO company);

        public Task<IList<PropertyUserDTO>> GetCompanyUsersAsync(string companyId);
        public Task<PropertyUserDTO> GetCompanyUserAsync(string companyId, string userId);

    }

    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IRoleRepository _roleRepository;

        public CompanyService(ICompanyRepository companyRepository, IUserService userService, IRoleService roleService, IRoleRepository roleRepository)
        {
            _companyRepository = companyRepository;
            _userService = userService;
            _roleService = roleService;
            _roleRepository = roleRepository;
        }

        public async Task<CompanyDTO> CreateCompanyAsync(CompanyLogin companyLogin)
        {
            var company = (await _companyRepository.FilterByAsync(x => x.Name.Equals(companyLogin.CompanyName))).FirstOrDefault();

            if (company is not null)
                throw new Exception("A Company with this name is already existing");

            var role = (await _roleRepository.FilterByAsync(x => x.Name.Equals("Firma"))).FirstOrDefault();

            if (role is null)
                role = await _roleService.CreateRoleAsync(new Role() { Name = "Firma" });

            var roleIds = new List<ObjectId>();
            roleIds.Add(role.Id);

            var companyUser = new User()
            {
                Firstname = companyLogin.CompanyName,
                Lastname = "Firma",
                HashedPassword = companyLogin.HashedPassword
            };

            var newUser = await _userService.CreateNewUserAsync(companyUser);

            PropertyUser propertyUser = new PropertyUser()
            {
                UserId = newUser.Id,
                RoleIds = roleIds
            };

            var propertyUsers = new List<PropertyUser>();
            propertyUsers.Add(propertyUser);

            company = new Company()
            {
                Name = companyLogin.CompanyName,
                Users = propertyUsers
            };

            await _companyRepository.CreateAsync(company);

            var roles = new List<RoleDTO>();
            roles.Add(new RoleDTO(role));

            var firmenUser = new PropertyUserDTO()
            {
                User = new UserDTO(newUser),
                Roles = roles
            };

            return new CompanyDTO() {Id = company.Id, Name = company.Name, User = firmenUser};
        }

        public async Task<IList<CompanyDTO>> GetCompaniesAsync()
        {
            var companies = await _companyRepository.GetAsync();

            if (companies is null)
                throw new Exception("Something went wrong");

            var companyDTOs = new List<CompanyDTO>();

            foreach(var company in companies)
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

            foreach(var propertyUser in company.Users)
            {
                var user = userList.FirstOrDefault(x => x.Id.Equals(propertyUser.UserId));

                IList<RoleDTO> roles = new List<RoleDTO>();

                foreach(var roleId in propertyUser.RoleIds)
                {
                    var role = roleList.FirstOrDefault(x => x.Id.Equals(roleId));

                    if(role is not null)
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

            var propertyUser = company.Users.FirstOrDefault(x => x.UserId.Equals(userId));

            if (propertyUser is null)
                throw new Exception("There is no User with this ID");

            var user = await _userService.GetUser(propertyUser.UserId);

            if(user is null)
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
    }
}
