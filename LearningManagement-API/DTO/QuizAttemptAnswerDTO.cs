namespace LearningManagement_API.DTO
{
    public class QuizAttemptAnswerDto
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public int UserId { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public double ScorePercentage { get; set; }
        public bool IsPassed { get; set; }

        public List<QuizAttemptAnswerDto> Answers { get; set; }
    }
}
