using Goose.Data.Models;
using Goose.Domain.Models.identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Goose.Domain.DTOs
{
    public class UserDTO
    {
        public ObjectId Id { get; set; }
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }

        public UserDTO()
        {

        }

        public UserDTO(User user)
        {
            Id = user.Id;
            Username = user.Username;
            Firstname = user.Firstname;
            Lastname = user.Lastname;
        }
    }
}