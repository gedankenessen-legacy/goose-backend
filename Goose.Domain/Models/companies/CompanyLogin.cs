using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Domain.Models.Companies
{
    public class CompanyLogin
    {
        [Required(ErrorMessage = "Company Name is Required")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Please provide a Password for the Company")]
        public string HashedPassword { get; set; }
    }
}
