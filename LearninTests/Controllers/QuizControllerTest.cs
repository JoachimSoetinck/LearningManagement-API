using LearningManagement_API.Data;
using LearningManagement_API.DTO;
using LearningManagement_API.DTO.Quiz;
using LearningManagement_API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningManagement_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizControllerTest : ControllerBase
    {
        private readonly LearningManagement_APIContext _context;

        public QuizControllerTest(LearningManagement_APIContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizDTO>>> GetAll()
        {
            var quizzes = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .Select(q => ToQuizDto(q))
                .ToListAsync();

            return Ok(quizzes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<QuizDTO>> GetById(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .Where(q => q.Id == id)
                .Select(q => ToQuizDto(q))
                .FirstOrDefaultAsync();

            if (quiz == null)
                return NotFound();

            return Ok(quiz);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<QuizDTO>> Create(CreateQuizDto dto)
        {
            var quiz = new Quiz
            {
                Title = dto.Title,
                TimeLimitInMinutes = dto.TimeLimitInMinutes,
                MaxAttemptsPerUser = dto.MaxAttemptsPerUser,
                PassingScorePercentage = dto.PassingScorePercentage,
                IsPublished = dto.IsPublished
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = quiz.Id },
                ToQuizDto(quiz)
            );
        }

        // =========================
        // UPDATE QUIZ (ADMIN ONLY)
        // =========================
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateQuizDto dto)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
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

        // =========================
        // DELETE QUIZ (ADMIN ONLY)
        // =========================
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
                return NotFound();

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =========================
        // MAPPING
        // =========================
        private static QuizDTO ToQuizDto(Quiz quiz)
        {
            return new QuizDTO
            {
                Id = quiz.Id,
                Title = quiz.Title,
                TimeLimitInMinutes = quiz.TimeLimitInMinutes,
                MaxAttemptsPerUser = quiz.MaxAttemptsPerUser,
                PassingScorePercentage = quiz.PassingScorePercentage,
                IsPublished = quiz.IsPublished,
                Questions = quiz.Questions.Select(q => new QuestionDTO
                {
                    Id = q.Id,
                    Text = q.Text,
                    AnswerOptions = q.AnswerOptions.Select(a => new AnswerDTO
                    {
                        Id = a.Id,
                        Text = a.Text
                    }).ToList()
                }).ToList()
            };
        }
    }
}
