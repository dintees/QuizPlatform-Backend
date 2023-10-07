namespace QuizPlatform.Infrastructure.Models.TestSession
{
    public  class UserAnswersDto
    {
        public int QuestionId { get; set; }
        public List<int>? AnswerIds { get; set; }
        public string? ShortAnswerValue { get; set; }
        public bool IsCorrect { get; set; }
    }
}
