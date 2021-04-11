using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Repositories;
using Goose.API.Utils.Authentication;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs;
using Goose.Domain.Models;
using Goose.Domain.Models.Identity;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace Goose.API.Services
{
    public interface ICompanyUserService
    {
        public Task<IList<PropertyUserDTO>> GetCompanyUsersAsync(string companyId);
        public Task<PropertyUserDTO> GetCompanyUserAsync(string companyId, string userId);
        public Task<PropertyUserDTO> CreateComapanyUserAsync(string companyId, PropertyUserLoginDTO user);
        public Task<PropertyUserDTO> UpdateComapanyUserAsync(string companyId, string userId, PropertyUserLoginDTO user);
    }

    public class CompanyUserService : ICompanyUserService
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly ICompanyRepository _companyRepository;
        private readonly IAuthService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompanyUserService(ICompanyRepository companyRepository, IUserService userService, IRoleService roleService, IAuthService authService, IHttpContextAccessor httpContextAccessor)
        {
            _companyRepository = companyRepository;
            _userService = userService;
            _roleService = roleService;
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PropertyUserDTO> CreateComapanyUserAsync(string companyId, PropertyUserLoginDTO user)
        {
            var company = await _companyRepository.GetCompanyByIdAsync(companyId);

            var requestUserId = _httpContextAccessor.HttpContext.User.GetUserId();

            var requestUser = await GetCompanyUserAsync(companyId, requestUserId.ToString());

            if (requestUser is null)
                throw new HttpStatusException(400, "Der angefragte Benutzer Existiert nicht in der Firma");

            if(requestUser.Roles.FirstOrDefault(x => x.Name.Equals(Role.CompanyRole)) is null)
                throw new HttpStatusException(400, "Nur der Firmen Account darf einen Benutzer erstellen");

            // create new user
            User newUser = new User()
            {
                Firstname = user.Firstname,
                Lastname = user.Lastname
            };

            // generate username
            newUser.Username = await _authService.GenerateUserNameAsync(newUser.Firstname, newUser.Lastname);

            // password hashed by BCrypt with workFactor of 11 => round about 150-300 ms depends on hardware.
            newUser.HashedPassword = _authService.GetHashedPassword(user.Password);

            // save user
            await _userService.CreateNewUserAsync(newUser);

            var roles = await GetRoleIds(user.Roles);

            company.Users.Add(new PropertyUser() {UserId = newUser.Id, RoleIds = roles});

            await _companyRepository.UpdateAsync(company);

            var roleList = await _roleService.GetRolesAsync();

            var roleDTOs = new List<RoleDTO>();

            foreach (var roleId in roles)
                roleDTOs.Add(new RoleDTO(roleList.FirstOrDefault(x => x.Id.Equals(roleId))));

            return new PropertyUserDTO() {User = new UserDTO(newUser), Roles = roleDTOs};
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

                propertyUserList.Add(new PropertyUserDTO() {User = user, Roles = roles});
            }

            return propertyUserList;
        }

        public async Task<PropertyUserDTO> GetCompanyUserAsync(string companyId, string userId)
        {
            var company = await _companyRepository.GetCompanyByIdAsync(companyId);

            var propertyUser = company.Users.FirstOrDefault(x => x.UserId.Equals(new ObjectId(userId)));

            if (propertyUser is null)
                throw new HttpStatusException(400, "Es wurde kein User mit dieser ID gefunden");

            var user = await _userService.GetUser(propertyUser.UserId);

            if (user is null)
                throw new HttpStatusException(400, "Es wurde kein User mit dieser ID gefunden");

            IList<RoleDTO> roles = new List<RoleDTO>();

            var roleList = await _roleService.GetRolesAsync();

            foreach (var roleId in propertyUser.RoleIds)
            {
                var role = roleList.FirstOrDefault(x => x.Id.Equals(roleId));

                if (role is not null)
                    roles.Add(new RoleDTO(role));
            }

            return new PropertyUserDTO() {User = user, Roles = roles};
        }

        public async Task<PropertyUserDTO> UpdateComapanyUserAsync(string companyId, string userId, PropertyUserLoginDTO user)
        {
            if (!userId.Equals(user.UserId))
                throw new HttpStatusException(400, "Die angegebene UserID stimmt nicht mit dem User Überein");

            var userToUpdate = new User()
            {
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                HashedPassword = _authService.GetHashedPassword(user.Password)
            };

            await _userService.UpdateUserAsync(new ObjectId(userId), userToUpdate);

            var roles = await GetRoleIds(user.Roles);

            var company = await _companyRepository.GetCompanyByIdAsync(companyId);
            var companyUser = company.Users.FirstOrDefault(x => x.UserId.Equals(userId));

            if (companyUser is null)
                throw new HttpStatusException(400, "Es wurde kein User mit dieser ID gefunden");

            companyUser.RoleIds = roles;

            await _companyRepository.UpdateAsync(company);

            var roleDTOs = new List<RoleDTO>();
            var roleList = await _roleService.GetRolesAsync();

            foreach (var roleId in roles)
                roleDTOs.Add(new RoleDTO(roleList.FirstOrDefault(x => x.Id.Equals(roleId))));

            return new PropertyUserDTO() { User = new UserDTO(userToUpdate), Roles = user.Roles };
        }

        private async Task<List<ObjectId>> GetRoleIds(IList<RoleDTO> roles)
        {
            var roleIds = new List<ObjectId>();

            var roleList = await _roleService.GetRolesAsync();

            foreach (var role in roles)
            {
                var roleFromDB = roleList.FirstOrDefault(x => x.Id.Equals(role.Id) || x.Name.Equals(role.Name));

                if (roleFromDB == null)
                {
                    roleFromDB = await _roleService.CreateRoleAsync(new Role() {Name = role.Name});
                }

                roleIds.Add(roleFromDB.Id);
            }

            return roleIds;
        }
    }
}