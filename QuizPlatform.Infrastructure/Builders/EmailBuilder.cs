using MimeKit;
using MimeKit.Text;
using QuizPlatform.Infrastructure.Authentication;

namespace QuizPlatform.Infrastructure.Builders
{
    public class EmailBuilder
    {
        private readonly EmailConfiguration _emailConfiguration;
        private string _recipient = string.Empty;
        private bool _isHtml = false;
        private string _subject = string.Empty;
        private string _message = string.Empty;

        public EmailBuilder(EmailConfiguration emailConfiguration)
        {
            _emailConfiguration = emailConfiguration;
        }

        public EmailBuilder To(string recipient)
        {
            _recipient = recipient;
            return this;
        }

        public EmailBuilder IsHtml(bool isHtml = true)
        {
            _isHtml = isHtml;
            return this;
        }

        public EmailBuilder WithSubject(string subject)
        {
            _subject = subject;
            return this;
        }

        public EmailBuilder WithMessage(string message)
        {
            _message = message;
            return this;
        }

        public MimeMessage Build()
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailConfiguration.FromDisplayName, _emailConfiguration.From));
            message.To.Add(new MailboxAddress(_recipient, _recipient));
            message.Subject = _subject;
            message.Body = new TextPart(_isHtml ? TextFormat.Html : TextFormat.Text) { Text = _message };

            return message;
        }
    }
}
