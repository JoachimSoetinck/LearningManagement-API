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
            int total = quiz.Questions.Count;

            foreach (var question in quiz.Questions)
            {
                var givenAnswer = dto.Answers
                    .FirstOrDefault(a => a.QuestionId == question.Id);

                if (givenAnswer?.SelectedAnswerOptionId == null)
                    continue;

                bool isCorrect = question.AnswerOptions
                    .Any(a =>
                        a.Id == givenAnswer.SelectedAnswerOptionId &&
                        a.IsCorrect);

                if (isCorrect)
                    correct++;
            }

            return Math.Round((double)correct / total * 100, 2);
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
