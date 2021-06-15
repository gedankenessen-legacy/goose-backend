using System;
using System.Net.Http;
using System.Threading.Tasks;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Issues;
using Goose.Domain.Models.Projects;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class IssueTests
    {
        private class SimpleTestHelperBuilderStartDate : SimpleTestHelperBuilder
        {
            public override IssueDTO GetIssueDTOCopy(HttpClient client, SimpleTestHelper helper)
            {
                IssueDTO issueCopy = base.GetIssueDTOCopy(client, helper);
                issueCopy.IssueDetail.RequirementsNeeded = false;
                issueCopy.IssueDetail.StartDate = DateTime.Now.AddSeconds(300);
                return issueCopy;
            }
        }
        
        [Test]
        public async Task BugIssueStartsInWaitingStateIfStartDateNotReached()
        {
            var helper = await new SimpleTestHelperBuilderStartDate().Build();
            Assert.AreEqual(State.WaitingState, helper.Issue.State.Name);
        }
    }
}