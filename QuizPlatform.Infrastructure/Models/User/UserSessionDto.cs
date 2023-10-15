namespace QuizPlatform.Infrastructure.Models.User
{
    public  class UserSessionDto
    {
        public DateTime LoggedInTime { get; set; }
        public string? IPAddress { get; set; }
        public string? Browser { get; set; }
    }
}
