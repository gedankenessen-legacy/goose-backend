using Goose.API;
using Goose.API.Repositories;
using Goose.Domain.Models.Auth;
using Goose.Domain.Models.Issues;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    [TestFixture]
    class IssueRequirementTests
    {
        private HttpClient _client;
        private WebApplicationFactory<Startup> _factory;
        private ICompanyRepository _companyRepository;
        private IUserRepository _userRepository;
        private IIssueRepository _issueRepository;
        private IProjectRepository _projectRepository;
        private SignInResponse signInObject;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new WebApplicationFactory<Startup>();
            _client = _factory.CreateClient();
            var scopeFactory = _factory.Server.Services.GetService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope())
            {
                _companyRepository = scope.ServiceProvider.GetService<ICompanyRepository>();
                _userRepository = scope.ServiceProvider.GetService<IUserRepository>();
                _projectRepository = scope.ServiceProvider.GetService<IProjectRepository>();
                _issueRepository = scope.ServiceProvider.GetService<IIssueRepository>();
            }
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await Clear();
        }

        [SetUp]
        public async Task Setup()
        {
            await Clear();
            await Generate();
        }

        [Test]
        public async Task AddRequirement()
        {
            var issue = await TestHelper.Instance.GetIssueAsync();
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            var uri = $"/api/issues/{issue.Id}/requirements/";
            var responce = await _client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            var exits = issue.IssueDetail.Requirements?.FirstOrDefault(x => x.Requirement.Equals("Die Application Testen")) != null;
            Assert.IsTrue(exits);
        }

        [Test]
        public async Task AddRequirementFalse()
        {
            var issue = await TestHelper.Instance.GetIssueAsync();
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = " " };
            var uri = $"/api/issues/{issue.Id}/requirements/";
            var responce = await _client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsFalse(responce.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            var exits = issue.IssueDetail.Requirements?.FirstOrDefault(x => x.Requirement.Equals("Die Application Testen")) != null;
            Assert.IsFalse(exits);
        }

        [Test]
        public async Task DeleteRequirement()
        {
            var issue = await TestHelper.Instance.GetIssueAsync();
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            var uri = $"/api/issues/{issue.Id}/requirements/";
            var responce = await _client.PostAsync(uri, issueRequirement.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            var addedRequirement = issue.IssueDetail.Requirements?.FirstOrDefault(x => x.Requirement.Equals("Die Application Testen"));
            uri = $"/api/issues/{issue.Id}/requirements/{addedRequirement.Id}";
            responce = await _client.DeleteAsync(uri);
            Assert.IsTrue(responce.IsSuccessStatusCode);

            issue = await TestHelper.Instance.GetIssueAsync();
            var exits = issue.IssueDetail.Requirements?.FirstOrDefault(x => x.Requirement.Equals("Die Application Testen")) != null;
            Assert.IsFalse(exits);
        }

        private async Task Clear()
        {
            await TestHelper.Instance.ClearAll();
        }

        private async Task Generate()
        {
            await TestHelper.Instance.GenerateAll(_client);
        }
    }
}
