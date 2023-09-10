namespace QuizPlatform.Infrastructure.Models.User;

public class UserRegisterDto
{
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Password { get; set; }
    public string? PasswordConfirmation { get; set; }
    public int RoleId { get; set; }
}