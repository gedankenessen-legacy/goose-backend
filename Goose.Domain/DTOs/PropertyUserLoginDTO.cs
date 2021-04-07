using Goose.Domain.Models.Identity;
using System.Collections.Generic;

namespace Goose.Domain.DTOs
{
    public class PropertyUserLoginDTO
    {
        public User User { get; set; }
        public IList<RoleDTO> Roles { get; set; }
    }
}
