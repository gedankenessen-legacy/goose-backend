using AutoMapper;
using Goose.API.Repositories;
using Goose.Domain.Models.identity;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Services
{
    public interface IRoleService
    {
        Task<Role> GetRoleAsync(ObjectId id);
        Task<IList<Role>> GetRolesAsync();
        Task<Role> CreateRoleAsync(Role role);
        Task<Role> UpdateRoleAsync(ObjectId Id, Role role);
    }

    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;

        public RoleService(IRoleRepository roleRepository, IMapper mapper)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        public async Task<Role> GetRoleAsync(ObjectId id)
        {
            var role = await _roleRepository.GetAsync(id);

            if (role == null)
                throw new Exception();

            return role;
        }

        public async Task<IList<Role>> GetRolesAsync()
        {
            return await _roleRepository.GetAsync();
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            if (role == null)
                throw new Exception();

            if (string.IsNullOrWhiteSpace(role.Name))
                throw new Exception();

            var roleItems = await _roleRepository.FilterByAsync(x => x.Name.Equals(role.Name));

            if (roleItems.Count > 0)
                throw new Exception();

            var newRole = new Role()
            {
                Name = role.Name
            };

            await _roleRepository.CreateAsync(newRole);

            return newRole;
        }

        public async Task<Role> UpdateRoleAsync(ObjectId id, Role role)
        {
            if (role.Id == id)
                throw new Exception();

            if (role == null)
                throw new Exception();

            if (string.IsNullOrWhiteSpace(role.Name))
                throw new Exception();

            var roleItems = await _roleRepository.FilterByAsync(x => x.Name.Equals(role.Name));

            if (roleItems.Count > 0)
                throw new Exception();

            var roleToUpdate = (await _roleRepository.FilterByAsync(x => x.Id == role.Id)).FirstOrDefault();

            if (roleToUpdate == null)
                throw new Exception();

            roleToUpdate.Name = role.Name;

            await _roleRepository.UpdateAsync(roleToUpdate);

            return roleToUpdate;
        }
    }
}
