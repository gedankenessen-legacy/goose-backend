using Goose.API;
using Goose.API.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Tests.Application.UnitTests.Company
{
    [TestFixture]
    public class CompanyUserTest
    {
        private HttpClient _client;
        private WebApplicationFactory<Startup> _factory;
        private ICompanyRepository _companyRepository;
        private const string FirmenName = "WillysFirma";


        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new WebApplicationFactory<Startup>();
            _client = _factory.CreateClient();
            var scopeFactory = _factory.Server.Host.Services.GetService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope())
            {
                _companyRepository = scope.ServiceProvider.GetService<ICompanyRepository>();
            }
        }

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test()
        {

        }
    }
}
