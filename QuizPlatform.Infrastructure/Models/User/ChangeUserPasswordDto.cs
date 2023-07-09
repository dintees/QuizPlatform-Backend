namespace QuizPlatform.Infrastructure.Models.User;

public class ChangeUserPasswordDto
{
    public string? OldPassword { get; set; }
    public string? NewPassword { get; set; }
    public string? NewPasswordConfirmation { get; set; }
}