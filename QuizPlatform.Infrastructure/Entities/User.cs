namespace QuizPlatform.Infrastructure.Entities;

public class User : Entity
{
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public bool AccountConfirmed { get; set; }
    public Role? Role { get; set; }
    public int RoleId { get; set; }
}