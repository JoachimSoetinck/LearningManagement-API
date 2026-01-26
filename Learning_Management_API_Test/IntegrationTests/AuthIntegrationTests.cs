using System.Net;
using System.Net.Http.Json;
using LearningManagement_API.DTO;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LearningManagement_API.Tests.Integration
{
    public class AuthIntegrationTests
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_ReturnsToken_WhenCredentialsAreValid()
        {
            var response = await _client.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest
                {
                    Email = "admin@test.com",
                    Password = "Admin123!"
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("token", body);
        }

        [Fact]
        public async Task Login_Returns401_WhenPasswordIsWrong()
        {
            var response = await _client.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest
                {
                    Email = "admin@test.com",
                    Password = "WrongPassword"
                });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
