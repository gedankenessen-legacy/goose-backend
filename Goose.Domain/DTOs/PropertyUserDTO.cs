using System.Collections.Generic;
﻿using Goose.Domain.Models.Identity;
using MongoDB.Bson;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Domain.DTOs
{
    public class PropertyUserDTO
    {
        public UserDTO User { get; set; }
        public IList<RoleDTO> Roles {get; set;}
    }
}
