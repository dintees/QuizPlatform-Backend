using FluentValidation;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.ErrorMessages;

namespace QuizPlatform.API.Validation
{
    public class TestValidator : AbstractValidator<Test>
    {
        public TestValidator()
        {
            RuleFor(e => e.Title).NotEmpty().WithMessage(TestErrorMessages.EmptySetTitle);
            RuleForEach(e => e.Questions).SetValidator(new QuestionValidator());
        }
    }
}
