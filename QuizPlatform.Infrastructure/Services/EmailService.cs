using QuizPlatform.Infrastructure.Authentication;

namespace QuizPlatform.Infrastructure.Services
{
    public class EmailService
    {
        private readonly EmailConfiguration _emailConfiguration;

        public EmailService(EmailConfiguration emailConfiguration)
        {
            _emailConfiguration = emailConfiguration;
        }


    }
}
