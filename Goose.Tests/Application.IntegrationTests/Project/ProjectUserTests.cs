using Goose.Domain.DTOs;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Tests.Application.IntegrationTests.Project
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class ProjectUserTests
    {
        [Test]
        public async Task UnassignedFromTicketsIfRemovedFromProject()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();

            var user = helper.User;
            var project = helper.Project;
            var issue = helper.Issue;

            // assign user to issue
            var url = $"api/issues/{issue.Id}/users/{user.Id}";
            var result = await helper.client.PutAsync(url, null);
            Assert.IsTrue(result.IsSuccessStatusCode);

            // remove user from project
            url = $"api/projects/{project.Id}/users/{user.Id}";
            result = await helper.client.DeleteAsync(url);
            Assert.IsTrue(result.IsSuccessStatusCode);

            // Check assigned users
            url = $"api/issues/{issue.Id}/users";
            var assignedUsers = await helper.client.GetAsync(url).Parse<IList<UserDTO>>();
            Assert.IsFalse(assignedUsers.Any(x => x.Id == user.Id));
        }
    }
}
