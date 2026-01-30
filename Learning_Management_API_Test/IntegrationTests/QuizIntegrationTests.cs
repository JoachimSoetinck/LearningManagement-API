using LearningManagement_API.Data;
using LearningManagement_API.DTO.Quiz;
using LearningManagement_API.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace LearningManagement_API.Tests.Integration
{
    public class QuizIntegrationTests
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public QuizIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
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

            response.EnsureSuccessStatusCode();

            var json = await response.Content
                .ReadFromJsonAsync<Dictionary<string, string>>();

            return json!["token"];
        }

        private async Task<int> CreateQuizWithQuestions(string token)
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PostAsJsonAsync(
                "/api/quiz",
                new CreateQuizDto
                {
                    Title = "Integration Test Quiz",
                    TimeLimitInMinutes = 10,
                    MaxAttemptsPerUser = 2,
                    PassingScorePercentage = 60,
                    IsPublished = true
                });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider
                .GetRequiredService<LearningManagement_APIContext>();

            Quiz quiz = await db.Quizzes
                .OrderByDescending(q => q.Id)
                .FirstAsync();

            quiz.Questions.Add(new Question
            {
                Text = "Question 1",
                AnswerOptions = new List<AnswerOption>
                {
                    new() { Text = "Correct", IsCorrect = true },
                    new() { Text = "Wrong", IsCorrect = false }
                }
            });

            quiz.Questions.Add(new Question
            {
                Text = "Question 2",
                AnswerOptions = new List<AnswerOption>
                {
                    new() { Text = "Wrong", IsCorrect = false },
                    new() { Text = "Correct", IsCorrect = true }
                }
            });

            await db.SaveChangesAsync();

            return quiz.Id;
        }

        private async Task<SubmitQuizDto> BuildValidSubmissionDto(int quizId)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider
                .GetRequiredService<LearningManagement_APIContext>();

            Quiz quiz = await db.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.AnswerOptions)
                .FirstAsync(q => q.Id == quizId);

            return new SubmitQuizDto
            {
                QuizId = quizId,
                Answers = quiz.Questions.Select(q => new SubmitAnswerDto
                {
                    QuestionId = q.Id,
                    SelectedAnswerOptionId =
                        q.AnswerOptions.First(a => a.IsCorrect).Id
                }).ToList()
            };
        }

        [Fact]
        public async Task CreateQuiz_Returns201_WhenAdminAuthenticated()
        {
            string token = await GetAdminToken();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PostAsJsonAsync(
                "/api/quiz",
                new CreateQuizDto
                {
                    Title = "Create Quiz Test",
                    TimeLimitInMinutes = 10,
                    MaxAttemptsPerUser = 2,
                    PassingScorePercentage = 60,
                    IsPublished = true
                });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task SubmitQuiz_Returns401_WhenNotAuthenticated()
        {
            var dto = new SubmitQuizDto
            {
                QuizId = 1,
                Answers = new()
                {
                    new() { QuestionId = 1, SelectedAnswerOptionId = 1 }
                }
            };

            var response =
                await _client.PostAsJsonAsync("/api/quiz/1/submit", dto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task SubmitQuiz_Returns200_WhenAuthenticated()
        {
            string token = await GetAdminToken();
            int quizId = await CreateQuizWithQuestions(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            SubmitQuizDto dto = await BuildValidSubmissionDto(quizId);

            var response =
                await _client.PostAsJsonAsync($"/api/quiz/{quizId}/submit", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SubmitQuiz_Returns400_WhenMaxAttemptsReached()
        {
            string token = await GetAdminToken();
            int quizId = await CreateQuizWithQuestions(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            SubmitQuizDto dto = await BuildValidSubmissionDto(quizId);

            Assert.Equal(
                HttpStatusCode.OK,
                (await _client.PostAsJsonAsync(
                    $"/api/quiz/{quizId}/submit", dto)).StatusCode);

            Assert.Equal(
                HttpStatusCode.OK,
                (await _client.PostAsJsonAsync(
                    $"/api/quiz/{quizId}/submit", dto)).StatusCode);

            Assert.Equal(
                HttpStatusCode.BadRequest,
                (await _client.PostAsJsonAsync(
                    $"/api/quiz/{quizId}/submit", dto)).StatusCode);
        }
    }
}
