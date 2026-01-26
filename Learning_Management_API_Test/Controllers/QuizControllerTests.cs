using LearningManagement_API.Controllers;
using LearningManagement_API.Data;
using LearningManagement_API.DTO;
using LearningManagement_API.DTO.Quiz;
using LearningManagement_API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            context.Quizzes.Add(new Quiz
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
                            new AnswerOption { Id = 1, Text = "Option A" },
                            new AnswerOption { Id = 2, Text = "Option B" }
                        }
                    }
                }
            });

            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task GetAll_ReturnsAllQuizzes()
        {
            var context = CreateDbContext();
            var controller = new QuizController(context);

            var result = await controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var quizzes = Assert.IsAssignableFrom<IEnumerable<QuizDTO>>(okResult.Value);

            Assert.Single(quizzes);
        }

        [Fact]
        public async Task GetById_ReturnsQuiz_WhenExists()
        {
            var context = CreateDbContext();
            var controller = new QuizController(context);

            var result = await controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var quiz = Assert.IsType<QuizDTO>(okResult.Value);

            Assert.Equal("Test Quiz", quiz.Title);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            var context = CreateDbContext();
            var controller = new QuizController(context);

            var result = await controller.GetById(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_AddsQuiz()
        {
            var context = CreateDbContext();
            var controller = new QuizController(context);

            var dto = new CreateQuizDto
            {
                Title = "New Quiz",
                TimeLimitInMinutes = 15,
                MaxAttemptsPerUser = 2,
                PassingScorePercentage = 60,
                IsPublished = true
            };

            var result = await controller.Create(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var quiz = Assert.IsType<QuizDTO>(createdResult.Value);

            Assert.Equal("New Quiz", quiz.Title);
            Assert.Equal(2, context.Quizzes.Count());
        }

        [Fact]
        public async Task Update_UpdatesQuiz_WhenExists()
        {
            var context = CreateDbContext();
            var controller = new QuizController(context);

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

            var quiz = context.Quizzes.First();
            Assert.Equal("Updated Quiz", quiz.Title);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenNotExists()
        {
            var context = CreateDbContext();
            var controller = new QuizController(context);

            var dto = new UpdateQuizDto
            {
                Title = "Does Not Matter",
                TimeLimitInMinutes = 10,
                MaxAttemptsPerUser = 1,
                PassingScorePercentage = 50,
                IsPublished = false
            };

            var result = await controller.Update(999, dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_RemovesQuiz_WhenExists()
        {
            var context = CreateDbContext();
            var controller = new QuizController(context);

            var result = await controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(context.Quizzes);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenNotExists()
        {
            var context = CreateDbContext();
            var controller = new QuizController(context);

            var result = await controller.Delete(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
