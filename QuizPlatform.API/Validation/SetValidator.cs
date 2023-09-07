using FluentValidation;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.ErrorMessages;

namespace QuizPlatform.API.Validation
{
    public class SetValidator : AbstractValidator<Set>
    {
        public SetValidator()
        {
            RuleFor(e => e.Title).NotEmpty().WithMessage(SetErrorMessages.EmptySetTitle);
            RuleForEach(e => e.Questions).SetValidator(new QuestionValidator());
        }
    }
}
