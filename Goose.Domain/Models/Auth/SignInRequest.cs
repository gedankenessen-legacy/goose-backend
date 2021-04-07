using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Domain.Models.Auth
{
    public class SignInRequest
    {
        [Required(ErrorMessage = "Please provide a Username.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Please provide a Password.")]
        public string Password { get; set; }
    }
}
