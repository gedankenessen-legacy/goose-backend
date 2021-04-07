using Goose.Domain.Models;
using Goose.Domain.Models.Companies;
using Goose.Domain.Models.Projects;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Domain.DTOs
{
    public class CompanyDTO
    {
        public ObjectId Id { get; set; }

        public string Name { get; set; }

        public PropertyUserDTO User { get; set; }

        public static explicit operator CompanyDTO(Company company)
        {
            if (company is null)
                return null;

            return new CompanyDTO() 
            {
                Id = company.Id,
                Name = company.Name, 
                
            };
        }
    }
}
