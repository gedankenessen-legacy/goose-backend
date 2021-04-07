using AutoMapper;
using Goose.API.Repositories;
using Goose.Domain.DTOs;
using Goose.Domain.Models.Identity;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Services
{
    public interface IUserService
    {
        Task<UserDTO> GetUser(ObjectId id);
        Task<IList<UserDTO>> GetUsersAsync();
        Task<User> CreateNewUserAsync(User user);
        Task<User> UpdateUserAsync(ObjectId id, User user);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<User> CreateNewUserAsync(User user)
        {
            if (user == null)
                throw new Exception();

            if (string.IsNullOrWhiteSpace(user.Firstname))
                throw new Exception();

            if (string.IsNullOrWhiteSpace(user.Lastname))
                throw new Exception();

            if (string.IsNullOrWhiteSpace(user.HashedPassword))
                throw new Exception();

            var newUser = new User()
            {
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                HashedPassword = user.HashedPassword
            };

            await _userRepository.CreateAsync(newUser);

            return newUser;
        }

        public async Task<UserDTO> GetUser(ObjectId id)
        {
            var user = await _userRepository.GetAsync(id);

            if (user == null)
                throw new Exception();

            return new UserDTO() { Id = user.Id, Firstname = user.Firstname, Lastname = user.Lastname};
        }

        public async Task<IList<UserDTO>> GetUsersAsync()
        {
            var userList = await _userRepository.GetAsync();

            IList<UserDTO> userDTOList = new List<UserDTO>();

            foreach (var user in userList)
                userDTOList.Add(new UserDTO() { Id = user.Id, Firstname = user.Firstname, Lastname = user.Lastname });

            return userDTOList;
        }

        public async Task<User> UpdateUserAsync(ObjectId id, User user)
        {
            if (user == null)
                throw new Exception();

            if (id != user.Id)
                throw new Exception();

            if (string.IsNullOrWhiteSpace(user.Firstname))
                throw new Exception();

            if (string.IsNullOrWhiteSpace(user.Lastname))
                throw new Exception();

            if (string.IsNullOrWhiteSpace(user.HashedPassword))
                throw new Exception();

            var userToUpdate = await _userRepository.GetAsync(user.Id);

            if (userToUpdate == null)
                throw new Exception();

            userToUpdate.Firstname = user.Firstname;
            userToUpdate.Lastname = user.Lastname;
            userToUpdate.HashedPassword = user.HashedPassword;

            await _userRepository.UpdateAsync(userToUpdate);

            return userToUpdate;
        }
    }
}
