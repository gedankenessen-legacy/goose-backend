using Goose.API.Utils.Validators;
using System.ComponentModel.DataAnnotations;

namespace Goose.Domain.Models.Auth
{
    public class SignUpRequest
    {
        [Required(ErrorMessage = "Please provide a firstname.")]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "Please provide a lastname.")]
        public string Lastname { get; set; }
        
        [Required]
        [PasswordValidator]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please provide a company name.")]
        public string CompanyName { get; set; }
    }
}
