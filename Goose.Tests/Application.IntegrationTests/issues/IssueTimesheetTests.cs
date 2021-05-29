using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Identity;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Tests.Application.IntegrationTests.Issues
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

        /// <summary>
        /// Dieser Test überprüft, das beim starten einer neuen Zeiterfassung die Alte gestoppt wird.
        /// Der Ablauf:
        /// 1. Der Nutzer erstellt eine Zeiterfassung, und stoppt sie.
        /// 2. Danash startet er eine zweite Zeiterfassung ohne sie zu stoppen.
        /// 3. Dann startet er eine dritte ZE für ein anderes Ticket, wodurch die zweite ZE gestoppt wird.
        /// Die erste ZE soll unverändert bleiben.
        /// Außerdem wird noch für einen anderen Nutzer eine ZE gestartet, um zu überprüfen,
        /// das Diese bei Schritt 3 nicht beendet wird.
        /// </summary>
        /// <returns></returns>
        [Test]
        public static async Task TimeSheetCancellation()
        {
            var now = new DateTime(2020, 5, 19, 12, 0, 0, DateTimeKind.Local);

            // Create all the timesheets
            using var helper = await new SimpleTestHelperBuilder().Build();
            var secondaryUser = helper.User;

            var issueId = helper.Issue.Id;
            var timeSheetsUri = $"/api/issues/{issueId}/timesheets/";

            var firstTimeSheet = new IssueTimeSheetDTO()
            {
                User = secondaryUser,
                Start = now,
            };

            var result = await helper.client.PostAsync(timeSheetsUri, firstTimeSheet.ToStringContent());
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);

            var primaryUser = await helper.GenerateUserAndSetToProject(Role.EmployeeRole);
            Assert.AreNotEqual(secondaryUser.Id, primaryUser.Id);

            var secondTimeSheet = new IssueTimeSheetDTO()
            {
                User = primaryUser,
                Start = now.AddDays(-1),
                End = now.AddHours(-2),
            };

            result = await helper.client.PostAsync(timeSheetsUri, secondTimeSheet.ToStringContent());
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);

            var thirdTimeSheet = new IssueTimeSheetDTO()
            {
                User = primaryUser,
                Start = now.AddHours(-1),
                // Open ended
            };

            result = await helper.client.PostAsync(timeSheetsUri,thirdTimeSheet.ToStringContent());
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);

            var content = await helper.CreateIssue();
            var otherIssue = await content.Parse<IssueDTO>();
            var timeSheetsOfOtherIssueUri = $"/api/issues/{otherIssue.Id}/timesheets/";

            var fourthTimeSheet = new IssueTimeSheetDTO()
            {
                User = primaryUser,
                Start = now,
                // Open ended
            };

            result = await helper.client.PostAsync(timeSheetsOfOtherIssueUri, fourthTimeSheet.ToStringContent());
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);


            // check the state of all timesheets
            result = await helper.client.GetAsync(timeSheetsUri);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var timeSheets = await result.Parse<IList<IssueTimeSheetDTO>>();
            Assert.AreEqual(3, timeSheets.Count);

            AssertTimeSheetsAreEqualExceptId(firstTimeSheet, timeSheets[0]);
            AssertTimeSheetsAreEqualExceptId(secondTimeSheet, timeSheets[1]);
            Assert.AreEqual(thirdTimeSheet.User.Id, timeSheets[2].User.Id);
            Assert.AreEqual(thirdTimeSheet.Start, timeSheets[2].Start);
            Assert.AreNotEqual(default(DateTime), timeSheets[2].End); // end time must have been set

            // check other timesheet
            result = await helper.client.GetAsync(timeSheetsOfOtherIssueUri);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var otherTimeSheets = await result.Parse<IList<IssueTimeSheetDTO>>();
            Assert.AreEqual(1, otherTimeSheets.Count);

            AssertTimeSheetsAreEqualExceptId(fourthTimeSheet, otherTimeSheets[0]);
        }

        [Test]
        public async Task TimeSheetTest1()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();
            IssueTimeSheetDTO timeSheetDTO = new IssueTimeSheetDTO()
            {
                User = helper.User,
                Start = DateTime.Now
            };

            var uri = $"/api/issues/{helper.Issue.Id}/timesheets";
            var responce = await helper.client.PostAsync(uri, timeSheetDTO.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);
            var result = await responce.Parse<IssueTimeSheetDTO>();

            uri = $"/api/issues/{helper.Issue.Id}/timesheets/{result.Id}";
            responce = await helper.client.GetAsync(uri);
            Assert.IsTrue(responce.IsSuccessStatusCode);
            var createdSheet = await responce.Parse<IssueTimeSheetDTO>();

            createdSheet.End = DateTime.Now.AddHours(2);
            responce = await helper.client.PutAsync(uri, createdSheet.ToStringContent());
            Assert.IsTrue(responce.IsSuccessStatusCode);

            var issue = await helper.Helper.GetIssueThroughClientAsync(helper.Issue);
            Assert.AreEqual(2, issue.IssueDetail.TotalWorkTime);
        }

        public static void AssertTimeSheetsAreEqualExceptId(IssueTimeSheetDTO expected, IssueTimeSheetDTO actual)
        {
            Assert.AreEqual(expected.User.Id, actual.User.Id);
            Assert.AreEqual(expected.Start, actual.Start);
            Assert.AreEqual(expected.End, actual.End);
        }
    }
}
