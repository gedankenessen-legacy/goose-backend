using Goose.API;
using Goose.API.Repositories;
using Goose.API.Utils;
using Goose.Domain.DTOs;
using Goose.Domain.Models.Auth;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Projects;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Goose.Tests.Application.IntegrationTests.Project
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class ProjectAuthorizationTests
    {
        #region T10

        [Test]
        public async Task AssignCustomerToProjektAsCustomerTest()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();

            var user = await helper.Helper.CreateUserForCompany(helper.Company.Id);

            await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.CustomerRole);

            var response = await helper.Helper.AddUserToProject(helper.Project.Id, user.User.Id, Role.CustomerRole);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, "Customer was able to create a customer for a project!");
        }

        [Test]
        public async Task AssignCustomerToProjektAsOwnerTest()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();

            var user = await helper.Helper.CreateUserForCompany(helper.Company.Id);

            var response = await helper.Helper.AddUserToProject(helper.Project.Id, user.User.Id, Role.CustomerRole);

            Assert.IsTrue(response.IsSuccessStatusCode, "Company owner was not be able to create a customer for a project!");
        }

        #endregion

        #region T82

        [Test]
        public async Task EmployeeCreatesOwnState()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();

            await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.EmployeeRole);

            var response = await helper.Helper.CreateStateInProject(helper.Project.Id);

            Assert.IsTrue(response.IsSuccessStatusCode, "Employee was not able to create a custom state in his project!");
        }

        [Test]
        public async Task EmployeeEditsOwnState()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();

            await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.EmployeeRole);

            var response = await helper.Helper.CreateStateInProject(helper.Project.Id);

            var newState = await response.Parse<StateDTO>();

            newState.Name = "edited";

            response = await helper.Helper.EditStateInProject(helper.Project.Id, newState);

            response.EnsureSuccessStatusCode();

            Assert.True(response.IsSuccessStatusCode, $"{response.StatusCode}: Employee was not able to edit a custom state in his project!");
        }

        [Test]
        public async Task EmployeeRemovesOwnState()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();

            await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.EmployeeRole);

            var response = await helper.Helper.CreateStateInProject(helper.Project.Id);

            var newState = await response.Parse<StateDTO>();

            response = await helper.Helper.RemoveStateInProject(helper.Project.Id, newState);

            response.EnsureSuccessStatusCode();

            Assert.True(response.IsSuccessStatusCode, $"{response.StatusCode}: Employee was not able to remove a custom state in his project!");
        }

        [Test]
        public async Task CustomerIsNotAllowedToCreatesOwnState()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();

            await helper.Helper.GenerateUserAndSetToProject(helper.Company.Id, helper.Project.Id, Role.CustomerRole);

            var response = await helper.Helper.CreateStateInProject(helper.Project.Id);

            var error = await response.Content.Parse<ErrorResponse>();

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, "Customer was able to create a custom state for a project!\n" + error.Message);
        }

        [Test]
        public async Task CustomerIsNotAllowedToEditsOwnState()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();

            var response = await helper.Helper.CreateStateInProject(helper.Project.Id);

            var newState = await response.Parse<StateDTO>();

            var user = await helper.Helper.CreateUserForCompany(helper.Company.Id);

            await helper.Helper.AddUserToProject(helper.Project.Id, user.User.Id, Role.CustomerRole);

            var signIn = await helper.Helper.SignIn(new SignInRequest() { Username = user.User.Username, Password = helper.Helper.UsedPasswordForTests });

            helper.Helper.SetAuth(signIn);

            newState.Name = "edited";

            response = await helper.Helper.EditStateInProject(helper.Project.Id, newState);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, "Customer was able to edit a custom state for a project!");
        }

        [Test]
        public async Task CustomerIsNotAllowedToRemovesOwnState()
        {
            using var helper = await new SimpleTestHelperBuilder().Build();

            var response = await helper.Helper.CreateStateInProject(helper.Project.Id);

            var newState = await response.Parse<StateDTO>();

            var user = await helper.Helper.CreateUserForCompany(helper.Company.Id);

            await helper.Helper.AddUserToProject(helper.Project.Id, user.User.Id, Role.CustomerRole);

            var signIn = await helper.Helper.SignIn(new SignInRequest() { Username = user.User.Username, Password = helper.Helper.UsedPasswordForTests });

            helper.Helper.SetAuth(signIn);

            response = await helper.Helper.RemoveStateInProject(helper.Project.Id, newState);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, "Customer was able to remove a custom state for a project!");
        }
        #endregion
    }

}
