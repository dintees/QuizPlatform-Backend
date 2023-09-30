using MimeKit;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface IEmailService
{
    Task SendAsync(MimeMessage message);
}