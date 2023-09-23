namespace QuizPlatform.Infrastructure.Models.TestSession
{
    public class UserTestSessionDto
    {
        public int Id { get; set; }
        public DateTime? TsInsert { get; set; }
        public DateTime? TsUpdate { get; set; }
        public string? TestName { get; set; }
    }
}
