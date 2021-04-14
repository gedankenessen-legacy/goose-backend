using Goose.API.Utils.Validators;
using Goose.Domain.Models.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Goose.Domain.DTOs
{
    public class PropertyUserLoginDTO
    {
        public string UserId { get; set; }

        [Required(ErrorMessage = "Please provide a firstname.")]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "Please provide a lastname.")]
        public string Lastname { get; set; }

        [Required]
        [PasswordValidator]
        public string Password { get; set; }

        public IList<RoleDTO> Roles { get; set; }
    }
}
