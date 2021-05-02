using System;
using System.Net.Http;
using System.Threading.Tasks;
using Goose.API;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class ParallelTestsTemplate
    {
        private sealed class TestScope : IDisposable
        {
            public HttpClient client;
            public WebApplicationFactory<Startup> _factory;
            public SimpleTestHelper Helper;

            public TestScope()
            {
                Task.Run(() =>
                {
                    _factory = new WebApplicationFactory<Startup>();
                    client = _factory.CreateClient();
                    Helper = new SimpleTestHelperBuilder(client).Build().Result;
                }).Wait();
            }

            public void Dispose()
            {
                client?.Dispose();
                _factory?.Dispose();
                Helper.Dispose();
            }
        }

        [Test]
        public async Task Test1()
        {
            using (var scope = new TestScope())
            {
            }
        }
    }
}