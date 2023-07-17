using FluentValidation;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.ErrorMessages;

namespace QuizPlatform.API.Validation
{
    public class SetValidator : AbstractValidator<Set>
    {
        public SetValidator()
        {
            RuleFor(e => e.Title).NotNull().WithMessage(SetErrorMessages.EmptySetTitle);
        }
    }
}
