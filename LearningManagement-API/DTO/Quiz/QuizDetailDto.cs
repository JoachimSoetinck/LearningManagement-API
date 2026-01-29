namespace LearningManagement_API.DTO.Quiz
{
    public class QuizDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TimeLimitInMinutes { get; set; }
        public int MaxAttemptsPerUser { get; set; }
        public int PassingScorePercentage { get; set; }
        public bool IsPublished { get; set; }

        public int QuestionCount { get; set; }
    }
}
