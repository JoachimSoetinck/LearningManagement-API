namespace LearningManagement_API.DTO
{
    public class QuestionDTO
    {
        public int Id { get; set; }
        public string Text { get; set; }

        public List<AnswerDTO> AnswerOptions { get; set; }
    }
}
