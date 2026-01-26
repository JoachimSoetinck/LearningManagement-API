using LearningManagement_API.Data;
using LearningManagement_API.DTO.Quiz;
using LearningManagement_API.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearningManagement_API.Helpers
{
    public class QuizSubmissionHelper
    {
        private readonly LearningManagement_APIContext _context;

        public QuizSubmissionHelper(LearningManagement_APIContext context)
        {
            _context = context;
        }

        public int GetUserId(ClaimsPrincipal user)
        {
            string? userId =
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not logged in");

            return int.Parse(userId);
        }

        public async Task<Quiz?> LoadQuizWithQuestionsAsync(int quizId)
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }

        public bool AllQuestionsAnswered(SubmitQuizDto dto, Quiz quiz)
        {
            return dto.Answers.Count == quiz.Questions.Count;
        }

        public double CalculateScore(SubmitQuizDto dto, Quiz quiz)
        {
            int correct = 0;

            foreach (SubmitAnswerDto answer in dto.Answers)
            {
                Question? question =
                    quiz.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);

                if (question == null)
                    throw new ArgumentException("Invalid question");

                AnswerOption? option =
                    question.AnswerOptions
                        .FirstOrDefault(a => a.Id == answer.SelectedAnswerOptionId);

                if (option == null)
                    throw new ArgumentException("Invalid answer option");

                if (option.IsCorrect)
                    correct++;
            }

            return (double)correct / quiz.Questions.Count * 100;
        }

        public QuizAttempt CreateQuizAttempt(
            SubmitQuizDto dto,
            Quiz quiz,
            int userId,
            double score,
            bool passed)
        {
            return new QuizAttempt
            {
                QuizId = quiz.Id,
                UserId = userId,
                CompletedAt = DateTime.UtcNow,
                ScorePercentage = score,
                IsPassed = passed,
                Answers = dto.Answers.Select(a => new QuizAttemptAnswer
                {
                    QuestionId = a.QuestionId,
                    SelectedAnswerOptionId = a.SelectedAnswerOptionId
                }).ToList()
            };
        }
    }
}
