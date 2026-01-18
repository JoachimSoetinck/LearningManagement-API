namespace LearningManagement_API.DTO.Quiz
{
    public class CreateQuizDto
    {
        public required string Title { get; set; }
        public int TimeLimitInMinutes { get; set; }
        public int MaxAttemptsPerUser { get; set; }
        public int PassingScorePercentage { get; set; }
        public bool IsPublished { get; set; }
    }
}
