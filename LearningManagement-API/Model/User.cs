using System.ComponentModel.DataAnnotations;

namespace LearningManagement_API.Model
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string FullName { get; set; } = null!;

        public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    }

}
