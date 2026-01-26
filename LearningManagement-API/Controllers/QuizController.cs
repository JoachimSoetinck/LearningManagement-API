using LearningManagement_API.Data;
using LearningManagement_API.DTO;
using LearningManagement_API.DTO.Quiz;
using LearningManagement_API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningManagement_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly LearningManagement_APIContext _context;

    public QuizController(LearningManagement_APIContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<QuizDTO>>> GetAll()
    {
        List<QuizDTO> quizzes = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.AnswerOptions)
            .Select(q => ToQuizDto(q))
            .ToListAsync();

        return Ok(quizzes);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<QuizDTO>> GetById(int id)
    {
        QuizDTO? quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.AnswerOptions)
            .Where(q => q.Id == id)
            .Select(q => ToQuizDto(q))
            .FirstOrDefaultAsync();

        if (quiz == null)
            return NotFound();

        return Ok(quiz);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<QuizDTO>> Create(CreateQuizDto dto)
    {
        Quiz quiz = new Quiz
        {
            Title = dto.Title,
            TimeLimitInMinutes = dto.TimeLimitInMinutes,
            MaxAttemptsPerUser = dto.MaxAttemptsPerUser,
            PassingScorePercentage = dto.PassingScorePercentage,
            IsPublished = dto.IsPublished
        };

        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        QuizDTO result = ToQuizDto(quiz);
        return CreatedAtAction(nameof(GetById), new { id = quiz.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateQuizDto dto)
    {
        Quiz? quiz = await _context.Quizzes.FindAsync(id);
        if (quiz == null)
            return NotFound();

        quiz.Title = dto.Title;
        quiz.TimeLimitInMinutes = dto.TimeLimitInMinutes;
        quiz.MaxAttemptsPerUser = dto.MaxAttemptsPerUser;
        quiz.PassingScorePercentage = dto.PassingScorePercentage;
        quiz.IsPublished = dto.IsPublished;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        Quiz? quiz = await _context.Quizzes.FindAsync(id);
        if (quiz == null)
            return NotFound();

        _context.Quizzes.Remove(quiz);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static QuizDTO ToQuizDto(Quiz q)
    {
        return new QuizDTO
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
                    .Select(a => new AnswerDTO
                    {
                        Id = a.Id,
                        Text = a.Text
                    }).ToList()
            }).ToList()
        };
    }
}
