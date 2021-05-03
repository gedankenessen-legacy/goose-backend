using Goose.API;
using Goose.Domain.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Tests.Application.IntegrationTests
{
    public sealed class TestScope : IDisposable
    {
        public HttpClient client;
        public WebApplicationFactory<Startup> _factory;
        public SimpleTestHelper Helper;

        public CompanyDTO Company;
        public ProjectDTO Project;

        public TestScope(SimpleTestHelperBuilderBase simpleTestHelperBuilder = null)
        {
            Task.Run(() =>
            {
                _factory = new WebApplicationFactory<Startup>();
                client = _factory.CreateClient();
                SimpleTestHelperBuilderBase testHelperBuilder = simpleTestHelperBuilder ?? new SimpleTestHelperBuilderBase();
                testHelperBuilder.SetClient(client);
                Helper = testHelperBuilder.Build().Result;
                Company = Helper.Company;
                Project = Helper.Project;
            }).Wait();
        }

        public void Dispose()
        {
            client?.Dispose();
            _factory?.Dispose();
            Helper.Dispose();
        }
    }
}
