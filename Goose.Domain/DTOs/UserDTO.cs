using Goose.Data.Models;
using Goose.Domain.Models.identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Domain.DTOs
{
    public class UserDTO : Document
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }

        public static implicit operator UserDTO(User user)
        {
            if (user is null)
                return null;

            return new UserDTO()
            {
                Id = user.Id,
                Firstname = user.Firstname,
                Lastname = user.Lastname
            };
        }
    }
}
