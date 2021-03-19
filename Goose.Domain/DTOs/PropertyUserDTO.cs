using System;
using System.Collections.Generic;
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
