namespace LearningManagement_API.DTO.Quiz
{
    public class QuizDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int TimeLimitInMinutes { get; set; }
        public int MaxAttemptsPerUser { get; set; }
        public int PassingScorePercentage { get; set; }
        public bool IsPublished { get; set; }

        public List<QuestionDTO> Questions { get; set; }
    }
}
