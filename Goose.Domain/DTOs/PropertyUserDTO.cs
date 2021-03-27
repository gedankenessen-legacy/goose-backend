using System.Collections.Generic;

namespace Goose.Domain.DTOs
{
    public class PropertyUserDTO
    {
        public UserDTO User { get; set; }
        public IList<RoleDTO> Roles {get; set;}
    }
}
