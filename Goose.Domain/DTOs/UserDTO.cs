using Goose.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Goose.Domain.Models.identity;

namespace Goose.Domain.DTOs
{
    public class UserDTO : Document
    {
        public UserDTO()
        {
        }

        public UserDTO(User user)
        {
            Firstname = user.Firstname;
            Lastname = user.Lastname;
        }

        public string Firstname { get; set; }
        public string Lastname { get; set; }
    }
}