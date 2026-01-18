using System.ComponentModel.DataAnnotations;

namespace LearningManagement_API.Model
{
    public class QuizAttempt
    {
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }
        [Required]
        public int UserId { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public double ScorePercentage { get; set; }
        public bool IsPassed { get; set; }

        public Quiz Quiz { get; set; } = null!;
        public ICollection<QuizAttemptAnswer> Answers { get; set; } = new List<QuizAttemptAnswer>();
    }

}
