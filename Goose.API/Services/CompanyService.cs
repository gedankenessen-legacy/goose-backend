using Goose.API.Repositories;
using Goose.Domain.DTOs;
using Goose.Domain.Models;
using Goose.Domain.Models.companies;
using Goose.Domain.Models.identity;
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
    }

    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private const string _companyRoleId = "604a3420db17824bca29698f";

        public CompanyService(ICompanyRepository companyRepository, IUserService userService, IRoleService roleService)
        {
            _companyRepository = companyRepository;
            _userService = userService;
            _roleService = roleService;
        }

        public async Task<CompanyDTO> CreateCompanyAsync(CompanyLogin companyLogin)
        {
            var company = (await _companyRepository.FilterByAsync(x => x.Name.Equals(companyLogin.CompanyName))).FirstOrDefault();

            if (company is not null)
                throw new Exception("A Company with this name is already existing");

            var role = await _roleService.GetRoleAsync(new ObjectId(_companyRoleId));

            if (role is null)
                throw new Exception();

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
                _id = new ObjectId(),
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

            var roles = new List<Role>();
            roles.Add(role);

            var firmenUser = new PropertyUserDTO()
            {
                Id = new ObjectId(),
                User = newUser,
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

            if (company is null)
                throw new Exception("No Company with this Id exists");

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
    }
}
