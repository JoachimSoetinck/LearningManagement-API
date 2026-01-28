namespace LearningManagement_API.DTO.Quiz
{
    public class QuizOverviewDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TimeLimitInMinutes { get; set; }
        public int MaxAttemptsPerUser { get; set; }
        public double PassingScorePercentage { get; set; }
    }
}
