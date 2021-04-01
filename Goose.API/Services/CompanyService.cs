using Goose.API.Repositories;
using Goose.API.Utils.Exceptions;
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
        private readonly IRoleRepository _roleRepository;
        private readonly ICompanyUserService _companyUserService;
        

        public CompanyService(ICompanyRepository companyRepository, IUserService userService, IRoleService roleService, IRoleRepository roleRepository)
        {
            _companyRepository = companyRepository;
            _userService = userService;
            _roleService = roleService;
            _roleRepository = roleRepository;
            _companyUserService = companyUserService;
        }

        public async Task<CompanyDTO> CreateCompanyAsync(CompanyLogin companyLogin)
        {
            var company = (await _companyRepository.FilterByAsync(x => x.Name.Equals(companyLogin.CompanyName))).FirstOrDefault();

            if (company is not null)
                throw new HttpStatusException(400, "Eine Company mit diesen Namen existiert bereit");

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
                throw new HttpStatusException(400, "Etwas ist schief gelaufen");

            var companyDTOs = new List<CompanyDTO>();

            foreach(var company in companies)
            {
                var companyDTO = (CompanyDTO)company;
                companyDTO.User = (await _companyUserService.GetCompanyUsersAsync(company.Id.ToString()))
                    .FirstOrDefault(x => x.Roles.FirstOrDefault(companyRole => companyRole.Name.Equals("Firma")) is not null);
                companyDTOs.Add(companyDTO);
            }

            return companyDTOs;
        }

        public async Task<CompanyDTO> GetCompanyAsync(string companyId)
        {
            var company = await _companyRepository.GetCompanyByIdAsync(companyId);

            var companyDTO = (CompanyDTO)company;

            companyDTO.User = (await _companyUserService.GetCompanyUsersAsync(company.Id.ToString()))
                    .FirstOrDefault(x => x.Roles.FirstOrDefault(companyRole => companyRole.Name.Equals("Firma")) is not null);

            return companyDTO;
        }

        public async Task<CompanyDTO> UpdateCompanyAsync(string id, CompanyDTO company)
        {
            if (company is null)
                throw new HttpStatusException(400, "Etwas ist schiefgelaufen");

            if (!id.Equals(company.Id))
                throw new HttpStatusException(400, "Die mitgebene ID stimmt nicht mit der company überein");

            var companyToUpdate = await _companyRepository.GetCompanyByIdAsync(id);

            if (companyToUpdate is null)
                throw new HttpStatusException(400, "Die mitgegebene Company existiert");

            companyToUpdate.Name = company.Name;

            await _companyRepository.UpdateAsync(companyToUpdate);

            return (CompanyDTO)companyToUpdate;
        }
    }
}
