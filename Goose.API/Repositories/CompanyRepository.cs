using Goose.Data.Context;
using Goose.Data.Repository;
using Goose.Domain.Models.Companies;
using MongoDB.Bson;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Repositories
{
    public interface ICompanyRepository : IRepository<Company>
    {
        public Task<Company> GetCompanyByIdAsync(string companyId);
    }

    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public CompanyRepository(IDbContext context) : base(context, "companies")
        {

        }

        public async Task<Company> GetCompanyByIdAsync(string companyId)
        {
            // check if the parsed objectId is not the 000...000 default objectId.
            if (ObjectId.TryParse(companyId, out ObjectId companyOid) is false)
                throw new Exception("Cannot parse company string id to a valid object id");

            var company = await GetAsync(companyOid);

            if(company is null)
                throw new Exception($"No Company found with Id = {companyId}");

            return company;
        }
    }
}
