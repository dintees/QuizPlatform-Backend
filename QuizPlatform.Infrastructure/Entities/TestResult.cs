namespace QuizPlatform.Infrastructure.Entities
{
    public class TestResult
    {
        public int Id { get; set; }
        public TestSession? TestSession { get; set; }
        public int TestSessionId { get; set; }
        public int Score { get; set; }
        public int MaxScore { get; set; }
    }
}
