using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
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
    class IssueDeadlineEventTests
    {
        private class SimpleTestHelperBuilderDeadline : SimpleTestHelperBuilder
        {
            public override IssueDTO GetIssueDTOCopy(HttpClient client, SimpleTestHelper helper)
            {
                IssueDTO issueCopy = base.GetIssueDTOCopy(client, helper);             
                issueCopy.IssueDetail.RequirementsNeeded = true;
                issueCopy.IssueDetail.StartDate = DateTime.Now;
                issueCopy.IssueDetail.EndDate = DateTime.Now.AddSeconds(5);
                return issueCopy;
            }
        }

        [Test]
        public async Task DeadLineReached()
        {
            using var helper = await new SimpleTestHelperBuilderDeadline().Build();

            var uri = $"/api/messages/{helper.User.Id}";
            var responce = await helper.client.GetAsync(uri);
            Assert.IsTrue(responce.IsSuccessStatusCode);
            var messageList = await responce.Parse<IList<MessageDTO>>();
            Assert.IsTrue(messageList.Count == 0);

            await Task.Delay(6000);

            uri = $"/api/messages/{helper.User.Id}";
            responce = await helper.client.GetAsync(uri);
            Assert.IsTrue(responce.IsSuccessStatusCode);
            messageList = await responce.Parse<IList<MessageDTO>>();
            Assert.IsTrue(messageList.Count == 2);
        }
    }
}
