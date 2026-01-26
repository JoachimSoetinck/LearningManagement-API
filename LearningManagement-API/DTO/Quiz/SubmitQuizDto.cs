namespace LearningManagement_API.DTO.Quiz
{
    public class SubmitQuizDto
    {
        public int QuizId { get; set; }

        public List<SubmitAnswerDto> Answers { get; set; }
            = new List<SubmitAnswerDto>();
    }

    public class SubmitAnswerDto
    {
        public int QuestionId { get; set; }

        public int SelectedAnswerOptionId { get; set; }
    }
}
