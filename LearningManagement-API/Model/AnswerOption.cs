using System.ComponentModel.DataAnnotations;

namespace LearningManagement_API.Model
{
    public class AnswerOption
    {
       
        public int Id { get; set; }
        [Required]
        public int QuestionId { get; set; }
        [Required]
        public string Text { get; set; } = null!;
        public bool IsCorrect { get; set; }

        [Required]
        public Question Question { get; set; } = null!;
    }

}
