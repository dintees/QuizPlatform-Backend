namespace QuizPlatform.Infrastructure.Authentication
{
    public class EmailConfiguration
    {
        public string? SmtpServer { get; set; }
        public int Port { get; set; }
        public string? From { get; set; }
        public string? FromDisplayName { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}
