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
    public interface IUserService
    {
        Task<User> GetUser(ObjectId id);
        Task<IList<User>> GetUsersAsync();
        Task<User> CreateNewUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
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

        public async Task<User> GetUser(ObjectId id)
        {
            var user = await _userRepository.GetAsync(id);

            if (user == null)
                throw new Exception();

            return user;
        }

        public async Task<IList<User>> GetUsersAsync()
        {
            return await _userRepository.GetAsync();
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            if (user == null)
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
