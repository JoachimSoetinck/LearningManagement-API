using LearningManagement_API.Data;
using LearningManagement_API.DTO.Quiz;
using LearningManagement_API.Helpers;
using LearningManagement_API.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Xunit;

namespace LearningManagement_API.Tests.Helpers
{
    public class QuizSubmissionHelperTests
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
                TimeLimitInMinutes = 10,
                MaxAttemptsPerUser = 3,
                PassingScorePercentage = 50,
                IsPublished = true,
                Questions = new List<Question>
                {
                    new Question
                    {
                        Id = 1,
                        Text = "Question 1",
                        AnswerOptions = new List<AnswerOption>
                        {
                            new AnswerOption
                            {
                                Id = 1,
                                Text = "Correct answer 1",
                                IsCorrect = true
                            },
                            new AnswerOption
                            {
                                Id = 2,
                                Text = "Wrong answer 1",
                                IsCorrect = false
                            }
                        }
                    },
                    new Question
                    {
                        Id = 2,
                        Text = "Question 2",
                        AnswerOptions = new List<AnswerOption>
                        {
                            new AnswerOption
                            {
                                Id = 3,
                                Text = "Correct answer 2",
                                IsCorrect = true
                            },
                            new AnswerOption
                            {
                                Id = 4,
                                Text = "Wrong answer 2",
                                IsCorrect = false
                            }
                        }
                    }
                }
            };

            context.Quizzes.Add(quiz);
            context.SaveChanges();

            return context;
        }

        private ClaimsPrincipal CreateUser(int userId)
        {
            Claim claim =
                new Claim(ClaimTypes.NameIdentifier, userId.ToString());

            ClaimsIdentity identity =
                new ClaimsIdentity(new List<Claim> { claim }, "TestAuth");

            return new ClaimsPrincipal(identity);
        }

        [Fact]
        public void AllQuestionsAnswered_ReturnsTrue_WhenAllAnswered()
        {
            LearningManagement_APIContext context = CreateDbContext();
            QuizSubmissionHelper helper =
                new QuizSubmissionHelper(context);

            Quiz quiz =
                context.Quizzes
                    .Include(q => q.Questions)
                    .First();

            SubmitQuizDto dto = new SubmitQuizDto
            {
                QuizId = 1,
                Answers = new List<SubmitAnswerDto>
                {
                    new SubmitAnswerDto
                    {
                        QuestionId = 1,
                        SelectedAnswerOptionId = 1
                    },
                    new SubmitAnswerDto
                    {
                        QuestionId = 2,
                        SelectedAnswerOptionId = 3
                    }
                }
            };

            bool result =
                helper.AllQuestionsAnswered(dto, quiz);

            Assert.True(result);
        }

        [Fact]
        public void CalculateScore_Returns100_WhenAllCorrect()
        {
            LearningManagement_APIContext context = CreateDbContext();
            QuizSubmissionHelper helper =
                new QuizSubmissionHelper(context);

            Quiz quiz =
                context.Quizzes
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.AnswerOptions)
                    .First();

            SubmitQuizDto dto = new SubmitQuizDto
            {
                QuizId = 1,
                Answers = new List<SubmitAnswerDto>
                {
                    new SubmitAnswerDto
                    {
                        QuestionId = 1,
                        SelectedAnswerOptionId = 1
                    },
                    new SubmitAnswerDto
                    {
                        QuestionId = 2,
                        SelectedAnswerOptionId = 3
                    }
                }
            };

            double score =
                helper.CalculateScore(dto, quiz);

            Assert.Equal(100d, score);
        }


        [Fact]
        public void CalculateScore_Returns50_WhenHalfCorrect()
        {
            LearningManagement_APIContext context = CreateDbContext();
            QuizSubmissionHelper helper =
                new QuizSubmissionHelper(context);

            Quiz quiz =
                context.Quizzes
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.AnswerOptions)
                    .First();

            SubmitQuizDto dto = new SubmitQuizDto
            {
                QuizId = 1,
                Answers = new List<SubmitAnswerDto>
        {
            // correct answer
            new SubmitAnswerDto
            {
                QuestionId = 1,
                SelectedAnswerOptionId = 1
            },
            // wrong answer
            new SubmitAnswerDto
            {
                QuestionId = 2,
                SelectedAnswerOptionId = 4
            }
        }
            };

            double score =
                helper.CalculateScore(dto, quiz);

            Assert.Equal(50d, score);
        }

    }


}
