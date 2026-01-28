using LearningManagement_API.Data;
using LearningManagement_API.DTO;
using LearningManagement_API.DTO.Quiz;
using LearningManagement_API.Helpers;
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
    private readonly QuizSubmissionHelper _helper;

    public QuizController(LearningManagement_APIContext context, QuizSubmissionHelper helper)
    {
        _context = context;
        _helper = helper;
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



    [HttpPost("{id}/submit")]
    [Authorize]
    public async Task<IActionResult> SubmitQuiz(int id, [FromBody] SubmitQuizDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (id != dto.QuizId)
            return BadRequest("QuizId mismatch");

        int userId = _helper.GetUserId(User);

        Quiz? quiz = await _helper.LoadQuizWithQuestionsAsync(id);
        if (quiz == null)
            return NotFound();

        if (!quiz.IsPublished)
            return BadRequest("Quiz is not published");

        if (!quiz.Questions.Any())
            return BadRequest("Quiz has no questions");

        if (!_helper.AllQuestionsAnswered(dto, quiz))
            return BadRequest("All questions must be answered");

        foreach (var answer in dto.Answers)
        {
            bool valid =
                quiz.Questions.Any(q =>
                    q.Id == answer.QuestionId &&
                    q.AnswerOptions.Any(a => a.Id == answer.SelectedAnswerOptionId));

            if (!valid)
                return BadRequest("Invalid answer option");
        }

        int attemptCount =
            await _context.QuizAttempts
                .CountAsync(a => a.QuizId == quiz.Id && a.UserId == userId);

        if (attemptCount >= quiz.MaxAttemptsPerUser)
            return BadRequest("Maximum number of attempts reached");

        double score = _helper.CalculateScore(dto, quiz);
        bool passed = score >= quiz.PassingScorePercentage;

        QuizAttempt attempt =
            _helper.CreateQuizAttempt(dto, quiz, userId, score, passed);

        _context.QuizAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            quizAttemptId = attempt.Id,
            scorePercentage = score,
            isPassed = passed
        });
    }


    [HttpGet("published")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<QuizDTO>>> GetPublished()
    {
      var quizzes = await _context.Quizzes
        .Where(q => q.IsPublished)
        .Select(q => new QuizOverviewDto
        {
            Id = q.Id,
            Title = q.Title,
            TimeLimitInMinutes = q.TimeLimitInMinutes,
            MaxAttemptsPerUser = q.MaxAttemptsPerUser,
            PassingScorePercentage = q.PassingScorePercentage
        })
        .ToListAsync();

    return Ok(quizzes);
    }


    [HttpPatch("{id}/publish")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PublishQuiz(int id)
    {
        var quiz = await _context.Quizzes.FindAsync(id);
        if (quiz == null)
            return NotFound();

        quiz.IsPublished = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }


    [HttpGet("{id}/attempts/me")]
    [Authorize]
    public async Task<IActionResult> GetMyAttempts(int id)
    {
        int userId = _helper.GetUserId(User);

        var attempts = await _context.QuizAttempts
            .Where(a => a.QuizId == id && a.UserId == userId)
            .Select(a => new
            {
                a.Id,
                a.ScorePercentage,
                a.IsPassed,
                a.CompletedAt
            })
            .ToListAsync();

        return Ok(attempts);
    }

    [HttpGet("{id}/attempts")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetQuizAttempts(int id)
    {
        var attempts = await _context.QuizAttempts
            .Where(a => a.QuizId == id)
            .Select(a => new
            {
                a.UserId,
                a.ScorePercentage,
                a.IsPassed,
                a.CompletedAt
            })
            .ToListAsync();

        return Ok(attempts);
    }



}
