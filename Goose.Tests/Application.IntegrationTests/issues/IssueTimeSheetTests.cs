using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Identity;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class IssueTimeSheetTests
    {
        #region GetTimeSheets

        [Test]
        public async Task CompanyCanGetTimeSheets() => await AssertCanGetTimeSheets(null);

        [Test]
        public async Task LeaderCanGetTimeSheets() => await AssertCanGetTimeSheets(Role.ProjectLeaderRole);

        [Test]
        public async Task EmployeeCanGetTimeSheets() => await AssertCanGetTimeSheets(Role.EmployeeRole);

        [Test]
        public async Task ReadonlyEmployeeCanGetTimeSheets() => await AssertCannotGetTimeSheets(Role.ReadonlyEmployeeRole);

        [Test]
        public async Task CustomerCanGetTimeSheets() => await AssertCannotGetTimeSheets(Role.CustomerRole);

        private async Task AssertCanGetTimeSheets(Role? role)
        {
            var res = await GetTimeSheetsSetUp(role);
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual(1, (await res.Parse<List<IssueTimeSheetDTO>>()).Count);
        }

        private async Task AssertCannotGetTimeSheets(Role? role)
        {
            var res = await GetTimeSheetsSetUp(role);
            Assert.AreEqual(HttpStatusCode.Forbidden, res.StatusCode);
        }

        private async Task<HttpResponseMessage> GetTimeSheetsSetUp(Role? role)
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            await helper.Helper.GenerateTimeSheet(helper.Issue.Id, helper.User.Id);

            if (role != null)
                await helper.GenerateUserAndSetToProject(role);

            var uri = $"api/issues/{helper.Issue.Id}/timesheets";
            return await helper.client.GetAsync(uri);
        }

        #endregion

        #region GetTimeSheet

        [Test]
        public async Task CompanyCanGetTimeSheet() => await AssertCanGetTimeSheet(null);

        [Test]
        public async Task LeaderCanGetTimeSheet() => await AssertCanGetTimeSheet(Role.ProjectLeaderRole);

        [Test]
        public async Task EmployeeCanGetTimeSheet() => await AssertCanGetTimeSheet(Role.EmployeeRole);

        [Test]
        public async Task ReadonlyEmployeeCanGetTimeSheet() => await AssertCannotGetTimeSheet(Role.ReadonlyEmployeeRole);

        [Test]
        public async Task CustomerCanGetTimeSheet() => await AssertCannotGetTimeSheet(Role.CustomerRole);

        private async Task AssertCanGetTimeSheet(Role? role)
        {
            var res = await GetTimeSheetSetUp(role);
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
        }

        private async Task AssertCannotGetTimeSheet(Role? role)
        {
            var res = await GetTimeSheetSetUp(role);
            Assert.AreEqual(HttpStatusCode.Forbidden, res.StatusCode);
        }

        private async Task<HttpResponseMessage> GetTimeSheetSetUp(Role? role)
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var timesheet = await helper.Helper.GenerateTimeSheet(helper.Issue.Id, helper.User.Id).Parse<IssueTimeSheetDTO>();

            if (role != null)
                await helper.GenerateUserAndSetToProject(role);

            var uri = $"api/issues/{helper.Issue.Id}/timesheets/{timesheet.Id}";
            return await helper.client.GetAsync(uri);
        }

        #endregion

        #region CreateTimeSheet

        [Test]
        public async Task CompanyCanCreateTimeSheet() => await AssertCanCreateTimeSheet(null);

        [Test]
        public async Task LeaderCanCreateTimeSheet() => await AssertCanCreateTimeSheet(Role.ProjectLeaderRole);

        [Test]
        public async Task EmployeeCanCreateTimeSheet() => await AssertCanCreateTimeSheet(Role.EmployeeRole);

        [Test]
        public async Task ReadonlyEmployeeCanCreateTimeSheet() => await AssertCannotCreateTimeSheet(Role.ReadonlyEmployeeRole);

        [Test]
        public async Task CustomerCanCreateTimeSheet() => await AssertCannotCreateTimeSheet(Role.CustomerRole);

        private async Task AssertCanCreateTimeSheet(Role? role)
        {
            var res = await CreateTimeSheetSetUp(role);
            Assert.AreEqual(HttpStatusCode.Created, res.StatusCode);
        }

        private async Task AssertCannotCreateTimeSheet(Role? role)
        {
            var res = await CreateTimeSheetSetUp(role);
            Assert.AreEqual(HttpStatusCode.Forbidden, res.StatusCode);
        }

        private async Task<HttpResponseMessage> CreateTimeSheetSetUp(Role? role)
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            if (role != null)
                await helper.GenerateUserAndSetToProject(role);
            return await helper.Helper.GenerateTimeSheet(helper.Issue.Id, helper.User.Id);
        }

        #endregion

        #region UpdateTimeSheet

        [Test]
        public async Task CompanyCanUpdateTimeSheet() => await UpdateTimeSheetSetUp(null, true);

        [Test]
        public async Task LeaderCanUpdateTimeSheet() => await UpdateTimeSheetSetUp(Role.ProjectLeaderRole, true);

        [Test]
        public async Task EmployeeCanUpdateTimeSheet() => await UpdateTimeSheetSetUp(Role.EmployeeRole, true);

        [Test]
        public async Task ReadonlyEmployeeCanUpdateTimeSheet() => await UpdateTimeSheetSetUp(Role.ReadonlyEmployeeRole, false);

        [Test]
        public async Task CustomerCanUpdateTimeSheet() => await UpdateTimeSheetSetUp(Role.CustomerRole, false);

        private async Task UpdateTimeSheetSetUp(Role? role, bool canUpdateTimeSheet)
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var timesheet = await helper.Helper.GenerateTimeSheet(helper.Issue.Id, helper.User.Id).Parse<IssueTimeSheetDTO>();
            timesheet.Start = DateTime.Now;
            timesheet.End = DateTime.Now;
            if (role != null)
                await helper.GenerateUserAndSetToProject(role);

            var uri = $"api/issues/{helper.Issue.Id}/timesheets/{timesheet.Id}";
            var res = await helper.client.PutAsync(uri, timesheet.ToStringContent());
            if (!canUpdateTimeSheet)
            {
                Assert.AreEqual(HttpStatusCode.Forbidden, res.StatusCode);
            }
            else
            {
                var newTimeSheet = await helper.client.GetAsync(uri).Parse<IssueTimeSheetDTO>();
                timesheet.AssertNotEqualJson(newTimeSheet);
            }
        }

        #endregion
        
        #region DeleteTimeSheet

        [Test]
        public async Task CompanyCanDeleteTimeSheet() => await DeleteTimeSheetSetUp(null, true);

        [Test]
        public async Task LeaderCanDeleteTimeSheet() => await DeleteTimeSheetSetUp(Role.ProjectLeaderRole, true);

        [Test]
        public async Task EmployeeCanDeleteTimeSheet() => await DeleteTimeSheetSetUp(Role.EmployeeRole, false);

        [Test]
        public async Task ReadonlyEmployeeCanDeleteTimeSheet() => await DeleteTimeSheetSetUp(Role.ReadonlyEmployeeRole, false);

        [Test]
        public async Task CustomerCanDeleteTimeSheet() => await DeleteTimeSheetSetUp(Role.CustomerRole, false);

        private async Task DeleteTimeSheetSetUp(Role? role, bool canDeleteTimeSheet)
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            var timesheet = await helper.Helper.GenerateTimeSheet(helper.Issue.Id, helper.User.Id).Parse<IssueTimeSheetDTO>();
            if (role != null)
                await helper.GenerateUserAndSetToProject(role);

            var uri = $"api/issues/{helper.Issue.Id}/timesheets/{timesheet.Id}";
            var res = await helper.client.DeleteAsync(uri);
            Assert.AreEqual(!canDeleteTimeSheet ? HttpStatusCode.Forbidden : HttpStatusCode.NoContent, res.StatusCode);
        }

        #endregion
    }
}