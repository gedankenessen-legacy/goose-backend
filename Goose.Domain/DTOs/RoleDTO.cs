using Goose.Domain.Models.Identity;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Domain.DTOs
{
    public class RoleDTO
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }

        public RoleDTO()
        {

        }

        public RoleDTO(Role role)
        {
            Id = role.Id;
            Name = role.Name;
        }
    }
}
