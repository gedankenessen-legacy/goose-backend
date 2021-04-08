using Goose.API.Utils.Validators;
using Microsoft.AspNetCore.Mvc;
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

        //[Remote(action: "CompanyNameAvailable", controller: "Company", ErrorMessage = "The provided company name is already taken.")]
        [Required(ErrorMessage = "Please provide a company name.")]  
        public string CompanyName { get; set; }
    }
}
