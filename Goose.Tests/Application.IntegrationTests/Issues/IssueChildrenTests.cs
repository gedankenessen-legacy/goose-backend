using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Goose.API.Utils;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Issues;
using Goose.Domain.Models.Projects;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class IssueChildrenTests
    {
        [Test]
        public async Task GetChildren()
        {
            var helper = await new SimpleTestHelperBuilder().Build();
            var child = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.SetIssueChild(child.Id);

            var res = await helper.Helper.GetChildrenIssues(helper.Issue.Id);
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            var children = await res.Parse<List<IssueDTO>>();
            Assert.AreEqual(1, children.Count);
            Assert.AreEqual(child.Id, children[0].Id);
        }

        [Test]
        public async Task GetChildrenRecursive()
        {
            var helper = await new SimpleTestHelperBuilder().Build();
            var child = await helper.CreateIssue().Parse<IssueDTO>();
            var childOfChild = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.SetIssueChild(child.Id);
            await helper.Helper.SetParentIssue(child.Id, childOfChild.Id);

            var res = await helper.Helper.GetChildrenIssues(helper.Issue.Id, true);
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            var children = await res.Parse<List<IssueDTO>>();
            Assert.AreEqual(2, children.Count);
            Assert.IsTrue(children.HasWhere(it => it.Id == child.Id));
            Assert.IsTrue(children.HasWhere(it => it.Id == childOfChild.Id));
        }

        [Test]
        public async Task AcceptChildrenSummary()
        {
            // Ober ticket Erstellen und Zusammenfassung akzeptieren
            using var helper = await new SimpleTestHelperBuilder().Build();
            await helper.SetState(State.NegotiationState);

            var issue = helper.Issue;
            IssueRequirement issueRequirement = new IssueRequirement() { Requirement = "Die Application Testen" };
            await helper.Helper.IssueRequirementService.CreateAsync(issue.Id, issueRequirement);

            var uri = $"/api/issues/{issue.Id}/summaries";
            var response = await helper.client.PostAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            uri = $"/api/issues/{issue.Id}/summaries?accept=true";
            response = await helper.client.PutAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            var newIssue = await helper.GetIssueAsync(issue.Id);
            Assert.IsTrue(newIssue.IssueDetail.RequirementsAccepted);

            Assert.AreEqual(State.ProcessingState, (await helper.Helper.GetStateById(newIssue)).Name);

            //Unterticket erstellen und Zusammenfassung acceptieren
            var child = await helper.CreateIssue().Parse<IssueDTO>();
            await helper.SetIssueChild(child.Id);

            await helper.Helper.SetStateOfIssue(child, State.NegotiationState);

            var res = await helper.Helper.GetChildrenIssues(helper.Issue.Id);
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            var children = await res.Parse<List<IssueDTO>>();
            Assert.AreEqual(1, children.Count);
            Assert.AreEqual(child.Id, children[0].Id);

            await helper.Helper.IssueRequirementService.CreateAsync(child.Id, issueRequirement);

            uri = $"/api/issues/{child.Id}/summaries";
            response = await helper.client.PostAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            uri = $"/api/issues/{child.Id}/summaries?accept=true";
            response = await helper.client.PutAsync(uri, 1.0.ToStringContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            var newChild = await helper.GetIssueAsync(child.Id);
            Assert.IsTrue(newIssue.IssueDetail.RequirementsAccepted);

            Assert.AreEqual(State.ProcessingState, (await helper.Helper.GetStateById(newChild)).Name);
        }
    }
}