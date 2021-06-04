using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Projects;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Tests.Application.IntegrationTests.Event
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    class IssueStartDateEventTests
    {
        private class SimpleTestHelperBuilderStartDate : SimpleTestHelperBuilder
        {
            public override IssueDTO GetIssueDTOCopy(HttpClient client, SimpleTestHelper helper)
            {
                IssueDTO issueCopy = base.GetIssueDTOCopy(client, helper);
                issueCopy.IssueDetail.RequirementsNeeded = true;
                issueCopy.IssueDetail.StartDate = DateTime.Now.AddSeconds(5);
                issueCopy.IssueDetail.EndDate = DateTime.Now.AddSeconds(10);
                return issueCopy;
            }
        }

        [Test]
        public async Task StartDateReached()
        {
            using var helper = await new SimpleTestHelperBuilderStartDate().Build();
            Assert.AreEqual(State.CheckingState, helper.Issue.State.Name);
            await helper.SetState(State.NegotiationState);

            await Task.Delay(6000);

            var uri = $"api/projects/{helper.Project.Id}/issues/{helper.Issue.Id}";
            var responce = await helper.client.GetAsync(uri);
            Assert.IsTrue(responce.IsSuccessStatusCode);
            var issueDTO = await responce.Parse<IssueDTO>();
            Assert.AreEqual(State.ProcessingState, issueDTO.State.Name);
        }
    }
}
