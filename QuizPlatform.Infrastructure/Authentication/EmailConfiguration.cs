namespace QuizPlatform.Infrastructure.Authentication
{
    public class EmailConfiguration
    {
        public string? ServerSmtp { get; set; }
        public int Port { get; set; }
        public string? From { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}
