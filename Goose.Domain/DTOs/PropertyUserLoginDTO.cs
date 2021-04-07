using Goose.Domain.Models.identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Domain.DTOs
{
    public class PropertyUserLoginDTO
    {
        public User User { get; set; }
        public IList<RoleDTO> Roles { get; set; }
    }
}
