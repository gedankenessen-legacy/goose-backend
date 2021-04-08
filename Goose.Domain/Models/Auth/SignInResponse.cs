using Goose.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Domain.Models.Auth
{
    public class SignInResponse
    {
        public UserDTO User { get; set; }
        public IList<CompanyDTO> Companies { get; set; }
        public string Token { get; set; }
    }
}
