using LearningManagement_API.DTO;
using LearningManagement_API.DTO.Quiz;
using LearningManagement_API.Model;

namespace LearningManagement_API.Mappings;

public static class QuizMappings
{
    public static QuizDTO ToQuizDto(this Quiz q) =>
        new()
        {
            Id = q.Id,
            Title = q.Title,
            TimeLimitInMinutes = q.TimeLimitInMinutes,
            MaxAttemptsPerUser = q.MaxAttemptsPerUser,
            PassingScorePercentage = q.PassingScorePercentage,
            IsPublished = q.IsPublished,
            Questions = q.Questions.Select(question => new QuestionDTO
            {
                Id = question.Id,
                Text = question.Text,
                AnswerOptions = question.AnswerOptions
                    .Select(static a => new AnswerDTO
                    {
                        Id = a.Id,
                        Text = a.Text
                    }).ToList()
            }).ToList()
        };
}
