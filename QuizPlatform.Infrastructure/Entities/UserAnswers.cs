namespace QuizPlatform.Infrastructure.Entities
{
    public class UserAnswers
    {
        public int Id { get; set; }
        public TestSession? TestSession { get; set; }
        public int TestSessionId { get; set; }
        public Question? Question { get; set; }
        public int QuestionId { get; set; }
        public QuestionAnswer? QuestionAnswer { get; set; }
        public int? QuestionAnswerId { get; set; }
        public string? ShortAnswerValue { get; set; }
    }
}
