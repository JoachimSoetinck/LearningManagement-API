using LearningManagement_API.Controllers;
using LearningManagement_API.Data;
using LearningManagement_API.DTO.Quiz;
using LearningManagement_API.Helpers;
using LearningManagement_API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Xunit;

namespace LearningManagement_API.Tests.Controllers
{
    public class QuizControllerTests
    {
        private LearningManagement_APIContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<LearningManagement_APIContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new LearningManagement_APIContext(options);

            var quiz = new Quiz
            {
                Id = 1,
                Title = "Test Quiz",
                TimeLimitInMinutes = 20,
                MaxAttemptsPerUser = 3,
                PassingScorePercentage = 70,
                IsPublished = true,
                Questions = new List<Question>
                {
                    new Question
                    {
                        Id = 1,
                        Text = "What is a test?",
                        AnswerOptions = new List<AnswerOption>
                        {
                            new AnswerOption { Id = 1, Text = "Option A", IsCorrect = true },
                            new AnswerOption { Id = 2, Text = "Option B", IsCorrect = false }
                        }
                    }
                }
            };

            context.Quizzes.Add(quiz);
            context.SaveChanges();

            return context;
        }

        private QuizController CreateControllerWithUser(LearningManagement_APIContext context)
        {
            var claim = new Claim(ClaimTypes.NameIdentifier, "1");
            var identity = new ClaimsIdentity(new[] { claim }, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext
            {
                User = principal
            };

            var controllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var helper = new QuizSubmissionHelper(context);
            var controller = new QuizController(context, helper)
            {
                ControllerContext = controllerContext
            };

            return controller;
        }

        [Fact]
        public async Task GetById_ReturnsQuiz_WhenExists()
        {
            var context = CreateDbContext();
            var controller = new QuizController(context, new QuizSubmissionHelper(context));

            var result = await controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var quiz = Assert.IsType<QuizDetailDto>(okResult.Value);

            Assert.Equal("Test Quiz", quiz.Title);
            Assert.True(quiz.IsPublished);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            var context = CreateDbContext();
            var controller = new QuizController(context, new QuizSubmissionHelper(context));

            var result = await controller.GetById(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_AddsQuiz()
        {
            var context = CreateDbContext();
            var controller = new QuizController(context, new QuizSubmissionHelper(context));

            var dto = new CreateQuizDto
            {
                Title = "New Quiz",
                TimeLimitInMinutes = 15,
                MaxAttemptsPerUser = 2,
                PassingScorePercentage = 60,
                IsPublished = true
            };

            IActionResult result = await controller.Create(dto);

            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(2, context.Quizzes.Count());

            var quiz = context.Quizzes.Last();
            Assert.Equal("New Quiz", quiz.Title);
            Assert.True(quiz.IsPublished);
        }

        [Fact]
        public async Task Update_UpdatesQuiz_WhenExists()
        {
            var context = CreateDbContext();
            var controller = new QuizController(context, new QuizSubmissionHelper(context));

            var dto = new UpdateQuizDto
            {
                Title = "Updated Quiz",
                TimeLimitInMinutes = 30,
                MaxAttemptsPerUser = 5,
                PassingScorePercentage = 80,
                IsPublished = false
            };

            var result = await controller.Update(1, dto);

            Assert.IsType<NoContentResult>(result);

            var updatedQuiz = context.Quizzes.First();
            Assert.Equal("Updated Quiz", updatedQuiz.Title);
            Assert.False(updatedQuiz.IsPublished);
        }

        [Fact]
        public async Task Delete_RemovesQuiz_WhenExists()
        {
            var context = CreateDbContext();
            var controller = new QuizController(context, new QuizSubmissionHelper(context));

            var result = await controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(context.Quizzes);
        }

        [Fact]
        public async Task SubmitQuiz_ReturnsPassed_WhenAllAnswersCorrect()
        {
            var context = CreateDbContext();
            var controller = CreateControllerWithUser(context);

            var dto = new SubmitQuizDto
            {
                QuizId = 1,
                Answers = new List<SubmitAnswerDto>
                {
                    new SubmitAnswerDto
                    {
                        QuestionId = 1,
                        SelectedAnswerOptionId = 1
                    }
                }
            };

            IActionResult result = await controller.SubmitQuiz(1, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            object value = okResult.Value!;

            var isPassedProp = value.GetType().GetProperty("isPassed");
            var scoreProp = value.GetType().GetProperty("scorePercentage");

            Assert.NotNull(isPassedProp);
            Assert.NotNull(scoreProp);

            bool passed = (bool)isPassedProp!.GetValue(value)!;
            double score = (double)scoreProp!.GetValue(value)!;

            Assert.True(passed);
            Assert.Equal(100d, score);
            Assert.Single(context.QuizAttempts);
        }
    }
}
