using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace Goose.API.Utils.Validators
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class PasswordValidatorAttribute : ValidationAttribute
    {
        public int MinLength { get; }
        public bool NumberRequired { get; }

        public PasswordValidatorAttribute(int minLength = 8, bool numberRequired = true)
        {
            MinLength = minLength;
            NumberRequired = numberRequired;

            ErrorMessage = "Please provide a valid password";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult(ErrorMessage);

            string password = value as string;

            // check for min length.
            if (password.Length < MinLength)
                return new ValidationResult($"The password must be at least {MinLength} characters long.");

            // check if a number is in the password.
            if (NumberRequired)        
                if (password.Any(char.IsDigit) is false)
                    return new ValidationResult($"Please add a number to the provided password.");         

            return ValidationResult.Success;
        }
    }
}
