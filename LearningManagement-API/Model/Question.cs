using System.ComponentModel.DataAnnotations;

namespace LearningManagement_API.Model
{
    public class Question
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        [Required]
        public string Text { get; set; } = null!;

        [Required]
        public Quiz Quiz { get; set; } = null!;
        public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
    }
}
