namespace QuizPlatform.Infrastructure.Entities
{
    public class UserAnswer : Entity
    {
        public Question? Question { get; set; }
        public int QuestionId { get; set; }
        public QuestionAnswer? QuestionAnswer { get; set; }
        public int QuestionAnswerId { get; set; }
        public User? User { get; set; }
        public int UserId { get; set; }
        public string? ShortAnswerValue { get; set; }
    }
}
