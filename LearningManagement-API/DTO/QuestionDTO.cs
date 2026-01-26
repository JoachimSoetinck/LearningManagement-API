namespace LearningManagement_API.DTO
{
    public class QuestionDTO
    {
        public int Id { get; set; }
        public required string Text { get; set; }

        public required List<AnswerDTO> AnswerOptions { get; set; }
    }
}
