using Goose.Data.Context;
using Goose.Data.Repository;
using Goose.Domain.Models.Companies;
using MongoDB.Bson;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Utils.Exceptions;

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
                throw new HttpStatusException(400, "Die mitgegebene ID lässt sich nicht in eine valiede Object ID parsen");

            var company = await GetAsync(companyOid);

            if(company is null)
                throw new HttpStatusException(400, "Die angeforderte Company existiert nicht");

            return company;
        }
    }
}
