namespace QuizPlatform.Infrastructure.Entities
{
    public class UserToken
    {
        public int Id { get; set; }
        public string? Token { get; set; }
        public User? User { get; set; }
        public int UserId { get; set; }
        public DateTime ExpirationTime { get; set; }
    }
}