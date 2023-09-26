namespace QuizPlatform.Infrastructure.Models.TestSession
{
    public  class UserSavedAnswersDto
    {
        public int QuestionId { get; set; }
        public List<int>? AnswerIds { get; set; }
        public string? ShortAnswerValue { get; set; }
    }
}
