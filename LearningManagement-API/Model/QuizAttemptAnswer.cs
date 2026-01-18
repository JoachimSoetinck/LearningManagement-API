namespace LearningManagement_API.Model
{
    using System.ComponentModel.DataAnnotations;

    public class QuizAttemptAnswer
    {
        public int Id { get; set; }

        [Required]
        public int QuizAttemptId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        public int SelectedAnswerOptionId { get; set; }

        [Required]
        public QuizAttempt QuizAttempt { get; set; } = null!;

        [Required]
        public Question Question { get; set; } = null!;

        [Required]
        public AnswerOption SelectedAnswerOption { get; set; } = null!;
    }


}
