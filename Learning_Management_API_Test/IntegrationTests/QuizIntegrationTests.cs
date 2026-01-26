using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LearningManagement_API.DTO.Quiz;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LearningManagement_API.Tests.Integration
{
    public class QuizIntegrationTests
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public QuizIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<string> GetAdminToken()
        {
            var response = await _client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    email = "admin@test.com",
                    password = "Admin123!"
                });

            var json = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            return json!["token"];
        }

        [Fact]
        public async Task CreateQuiz_Returns201_WhenAdminIsAuthenticated()
        {
            var token = await GetAdminToken();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PostAsJsonAsync(
                "/api/quiz",
                new CreateQuizDto
                {
                    Title = "Integration Test Quiz",
                    TimeLimitInMinutes = 10,
                    MaxAttemptsPerUser = 1,
                    PassingScorePercentage = 60,
                    IsPublished = true
                });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task CreateQuiz_Returns401_WhenNoTokenProvided()
        {
            var response = await _client.PostAsJsonAsync(
                "/api/quiz",
                new CreateQuizDto
                {
                    Title = "Unauthorized Quiz",
                    TimeLimitInMinutes = 10,
                    MaxAttemptsPerUser = 1,
                    PassingScorePercentage = 60,
                    IsPublished = true
                });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
