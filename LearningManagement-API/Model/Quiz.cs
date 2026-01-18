using System.ComponentModel.DataAnnotations;

namespace LearningManagement_API.Model
{
    public class Quiz
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; } = null!;

        [Required] 
        public int TimeLimitInMinutes { get; set; }
        [Required]
        public int MaxAttemptsPerUser { get; set; }
        [Required]
        public int PassingScorePercentage { get; set; }
        public bool IsPublished { get; set; }

        public ICollection<Question> Questions { get; set; } = new List<Question>();

    }
}
