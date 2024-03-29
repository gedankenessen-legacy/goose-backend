﻿using AutoMapper;
using Goose.API.Repositories;
using Goose.API.Utils.Exceptions;
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
        Task CreateNewUserAsync(User user);
        Task<User> UpdateUserAsync(ObjectId id, User user);
        Task<bool> UsernameAvailableAsync(string username);
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

        public async Task CreateNewUserAsync(User user)
        {
            if (user == null)
                throw new HttpStatusException(400, "Der mitgegebene User ist null");

            if (string.IsNullOrWhiteSpace(user.Firstname))
                throw new HttpStatusException(400, "Bitte geben Sie einen Vornamen für den User an");

            if (string.IsNullOrWhiteSpace(user.Lastname))
                throw new HttpStatusException(400, "Bitte geben Sie einen Nachnamen für den User an");

            if (string.IsNullOrWhiteSpace(user.HashedPassword))
                throw new HttpStatusException(400, "Bitte geben Sie ein Passwort für den User an");

            await _userRepository.CreateAsync(user);
        }

        public async Task<UserDTO> GetUser(ObjectId id)
        {
            var user = await _userRepository.GetAsync(id);

            if (user == null)
                throw new HttpStatusException(400, "Es wurde kein User mit dieser ID wurde nicht gefunden");

            return new UserDTO(user);
        }

        public async Task<IList<UserDTO>> GetUsersAsync()
        {
            var userList = await _userRepository.GetAsync();

            IList<UserDTO> userDTOList = new List<UserDTO>();

            foreach (var user in userList)
                userDTOList.Add(new UserDTO(user));

            return userDTOList;
        }

        public async Task<User> UpdateUserAsync(ObjectId id, User user)
        {
            if (user == null)
                throw new HttpStatusException(400, "Der mitgegebene User ist null");

            if (id != user.Id)
                throw new HttpStatusException(400, "die migegebene ID stimmt nicht mit der User ID überein");

            if (string.IsNullOrWhiteSpace(user.Firstname))
                throw new HttpStatusException(400, "Bitte geben Sie einen Vornamen für den User an");

            if (string.IsNullOrWhiteSpace(user.Lastname))
                throw new HttpStatusException(400, "Bitte geben Sie einen Nachnamen für den User an");

            if (string.IsNullOrWhiteSpace(user.HashedPassword))
                throw new HttpStatusException(400, "Bitte geben Sie ein Passwort für den User an");

            var userToUpdate = await _userRepository.GetAsync(user.Id);

            if (userToUpdate == null)
                throw new HttpStatusException(400, "Es wurde kein User mit dieser ID gefunden");

            userToUpdate.Firstname = user.Firstname;
            userToUpdate.Lastname = user.Lastname;
            userToUpdate.HashedPassword = user.HashedPassword;

            await _userRepository.UpdateAsync(userToUpdate);

            return userToUpdate;
        }

        public async Task<bool> UsernameAvailableAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username) is null;
        }
    }
}
