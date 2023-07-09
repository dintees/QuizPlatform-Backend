namespace QuizPlatform.Infrastructure.Entities;

public class UserSession
{
    public int Id { get; set; }
    public DateTime LoggedInTime { get; set; }
    public string? IPAddress { get; set; }
    public string? Browser { get; set; }
    public int UserId { get; set; }
    public virtual User? User { get; set; }
}