using System.Collections.Generic;
using Goose.Domain.Models.Identity;

namespace Goose.Domain.DTOs
{
    public class PropertyUserLoginDTO
    {
        public User User { get; set; }
        public IList<RoleDTO> Roles { get; set; }
    }
}
