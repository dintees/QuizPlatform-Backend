namespace QuizPlatform.Infrastructure.Models.User
{
    public class ForgotPasswordDto
    {
        public string? Email { get; set; }
        public string? NewPassword { get; set; }
        public string? NewPasswordConfirmation { get; set; }
    }
}
