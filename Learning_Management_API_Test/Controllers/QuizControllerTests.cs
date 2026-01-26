using LearningManagement_API.Controllers;
using LearningManagement_API.Data;
using LearningManagement_API.DTO;
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
            DbContextOptions<LearningManagement_APIContext> options =
                new DbContextOptionsBuilder<LearningManagement_APIContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options;

            LearningManagement_APIContext context =
                new LearningManagement_APIContext(options);

            Quiz quiz = new Quiz
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

        private QuizController CreateControllerWithUser(
            LearningManagement_APIContext context)
        {
            Claim claim = new Claim(ClaimTypes.NameIdentifier, "1");
            ClaimsIdentity identity =
                new ClaimsIdentity(new List<Claim> { claim }, "TestAuth");
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.User = principal;

            ControllerContext controllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            QuizSubmissionHelper helper =
                new QuizSubmissionHelper(context);

            QuizController controller =
                new QuizController(context, helper);

            controller.ControllerContext = controllerContext;

            return controller;
        }

        [Fact]
        public async Task GetAll_ReturnsAllQuizzes()
        {
            LearningManagement_APIContext context = CreateDbContext();
            QuizSubmissionHelper helper = new QuizSubmissionHelper(context);
            QuizController controller = new QuizController(context, helper);

            ActionResult<IEnumerable<QuizDTO>> result =
                await controller.GetAll();

            OkObjectResult okResult =
                Assert.IsType<OkObjectResult>(result.Result);

            IEnumerable<QuizDTO> quizzes =
                Assert.IsAssignableFrom<IEnumerable<QuizDTO>>(okResult.Value);

            Assert.Single(quizzes);
        }

        [Fact]
        public async Task GetById_ReturnsQuiz_WhenExists()
        {
            LearningManagement_APIContext context = CreateDbContext();
            QuizSubmissionHelper helper = new QuizSubmissionHelper(context);
            QuizController controller = new QuizController(context, helper);

            ActionResult<QuizDTO> result =
                await controller.GetById(1);

            OkObjectResult okResult =
                Assert.IsType<OkObjectResult>(result.Result);

            QuizDTO quiz =
                Assert.IsType<QuizDTO>(okResult.Value);

            Assert.Equal("Test Quiz", quiz.Title);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            LearningManagement_APIContext context = CreateDbContext();
            QuizSubmissionHelper helper = new QuizSubmissionHelper(context);
            QuizController controller = new QuizController(context, helper);

            ActionResult<QuizDTO> result =
                await controller.GetById(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_AddsQuiz()
        {
            LearningManagement_APIContext context = CreateDbContext();
            QuizSubmissionHelper helper = new QuizSubmissionHelper(context);
            QuizController controller = new QuizController(context, helper);

            CreateQuizDto dto = new CreateQuizDto
            {
                Title = "New Quiz",
                TimeLimitInMinutes = 15,
                MaxAttemptsPerUser = 2,
                PassingScorePercentage = 60,
                IsPublished = true
            };

            ActionResult<QuizDTO> result =
                await controller.Create(dto);

            CreatedAtActionResult createdResult =
                Assert.IsType<CreatedAtActionResult>(result.Result);

            QuizDTO quiz =
                Assert.IsType<QuizDTO>(createdResult.Value);

            Assert.Equal("New Quiz", quiz.Title);
            Assert.Equal(2, context.Quizzes.Count());
        }

        [Fact]
        public async Task Update_UpdatesQuiz_WhenExists()
        {
            LearningManagement_APIContext context = CreateDbContext();
            QuizSubmissionHelper helper = new QuizSubmissionHelper(context);
            QuizController controller = new QuizController(context, helper);

            UpdateQuizDto dto = new UpdateQuizDto
            {
                Title = "Updated Quiz",
                TimeLimitInMinutes = 30,
                MaxAttemptsPerUser = 5,
                PassingScorePercentage = 80,
                IsPublished = false
            };

            IActionResult result =
                await controller.Update(1, dto);

            Assert.IsType<NoContentResult>(result);

            Quiz updatedQuiz = context.Quizzes.First();
            Assert.Equal("Updated Quiz", updatedQuiz.Title);
        }

        [Fact]
        public async Task Delete_RemovesQuiz_WhenExists()
        {
            LearningManagement_APIContext context = CreateDbContext();
            QuizSubmissionHelper helper = new QuizSubmissionHelper(context);
            QuizController controller = new QuizController(context, helper);

            IActionResult result =
                await controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(context.Quizzes);
        }

        [Fact]
        public async Task SubmitQuiz_ReturnsPassed_WhenAllAnswersCorrect()
        {
            LearningManagement_APIContext context = CreateDbContext();
            QuizController controller = CreateControllerWithUser(context);

            SubmitQuizDto dto = new SubmitQuizDto
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

            IActionResult result =
                await controller.SubmitQuiz(1, dto);

            OkObjectResult okResult =
                Assert.IsType<OkObjectResult>(result);

            object value = okResult.Value!;

            double score =
                (double)value.GetType()
                    .GetProperty("scorePercentage")!
                    .GetValue(value)!;

            bool passed =
                (bool)value.GetType()
                    .GetProperty("isPassed")!
                    .GetValue(value)!;

            Assert.True(passed);
            Assert.Equal(100d, score);
            Assert.Single(context.QuizAttempts);
        }

        [Fact]
        public async Task SubmitQuiz_ReturnsBadRequest_WhenNotAllQuestionsAnswered()
        {
            LearningManagement_APIContext context = CreateDbContext();
            QuizController controller = CreateControllerWithUser(context);

            SubmitQuizDto dto = new SubmitQuizDto
            {
                QuizId = 1,
                Answers = new List<SubmitAnswerDto>()
            };

            IActionResult result =
                await controller.SubmitQuiz(1, dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
