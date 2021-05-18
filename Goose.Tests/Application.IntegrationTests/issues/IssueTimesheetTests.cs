using Goose.Domain.Models.Identity;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    class IssueTimeSheetTests
    {
        [Test]
        public async Task ReadTimeSheetsRightsTest()
        {
            var expected = new Dictionary<Role, HttpStatusCode>()
            {
                { Role.CustomerRole, HttpStatusCode.Forbidden },
                { Role.ReadonlyEmployeeRole, HttpStatusCode.OK },
                { Role.EmployeeRole, HttpStatusCode.OK },
                { Role.ProjectLeaderRole, HttpStatusCode.OK },
                { Role.CompanyRole, HttpStatusCode.OK },
            };

            foreach (var (role, expectedStatus) in expected)
            {
                var result = await GetTimeSheetsWithRole(role);
                var actualStatus = result.StatusCode;
                Assert.AreEqual(
                    expectedStatus,
                    actualStatus,
                    $"Expected: {expectedStatus}\nBut was: {actualStatus}\nFor Role {role.Name}");
            }
        }

        public static async Task<HttpResponseMessage> GetTimeSheetsWithRole(Role role)
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            await helper.GenerateUserAndSetToProject(role);

            var issueId = helper.Issue.Id;
            var uri = $"/api/issues/{issueId}/timesheets/";

            return await helper.client.GetAsync(uri);
        }
    }
}
