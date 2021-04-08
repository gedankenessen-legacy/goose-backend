using Goose.Domain.Models.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Tests.Application.IntegrationTests.Validators
{
    [TestFixture]
    public class PasswordValidatorAttributeControllerTests
    {
        private HttpClient _client;
        private WebApplicationFactory<Goose.API.Startup> _factory;

        [SetUp]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Goose.API.Startup>();
            _client = _factory.CreateClient();
        }

        [Test]
        public async Task PasswordTooShortTestAsync()
        {
            var uri = "/api/auth/signIn";
            SignUpRequest signUpRequest = new SignUpRequest() { Firstname = "a", Lastname = "b", CompanyName = "c", Password = "toShort" };

            var json = JsonConvert.SerializeObject(signUpRequest);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(uri, stringContent);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Request does not return false as intended.");
        }

        // Disabled: response.StatusCode is a bad distinguisher, could also been thrown by service, even if attribute returns success.
        // TODO: find a good way to validate success
        //[Test]
        //public async Task PasswordNotTooShortTestAsync()
        //{
        //    var uri = "/api/auth/signIn";
        //    SignUpRequest signUpRequest = new SignUpRequest() { Firstname = "a", Lastname = "b", CompanyName = "c", Password = "longAndWithNumber0" };

        //    var json = JsonConvert.SerializeObject(signUpRequest);
        //    var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
        //    var response = await _client.PostAsync(uri, stringContent);

        //    Assert.AreNotEqual(HttpStatusCode.BadRequest, response.StatusCode, "Request does not return false as intended.");
        //}
    }
}
