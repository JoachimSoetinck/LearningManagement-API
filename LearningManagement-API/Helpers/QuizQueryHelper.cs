using LearningManagement_API.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningManagement_API.Helpers;

public class QuizQueryHelper
{
    private readonly LearningManagement_APIContext _context;

    public QuizQueryHelper(LearningManagement_APIContext context)
    {
        _context = context;
    }

    public async Task<object?> GetQuizForTakingAsync(int quizId)
    {
        return await _context.Quizzes
            .Where(q => q.Id == quizId && q.IsPublished)
            .Select(q => new
            {
                id = q.Id,
                title = q.Title,
                questions = q.Questions.Select(question => new
                {
                    id = question.Id,
                    text = question.Text,
                    answerOptions = question.AnswerOptions.Select(a => new
                    {
                        id = a.Id,
                        text = a.Text
                    })
                })
            })
            .FirstOrDefaultAsync();
    }
}
