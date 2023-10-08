using FluentValidation;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Enums;
using QuizPlatform.Infrastructure.ErrorMessages;

namespace QuizPlatform.API.Validation
{
    public class QuestionValidator : AbstractValidator<Question>
    {
        public QuestionValidator()
        {
            RuleFor(e => e.Content).NotEmpty().WithMessage(QuestionErrorMessages.EmptyQuestionContent);
            RuleFor(m => m.QuestionType).IsInEnum().WithMessage(QuestionErrorMessages.InvalidQuestionType);
            RuleFor(e => e).Custom((e, context) =>
            {
                if (e.QuestionType == QuestionType.MultipleChoice || e.QuestionType == QuestionType.SingleChoice)
                {
                    if (e.Answers is null || e.Answers.Count == 0)
                    {
                        context.AddFailure("Answers", QuestionErrorMessages.EmptyAnswersContent);
                    }
                }

                if (e.QuestionType == QuestionType.SingleChoice) {
                    var count = e.Answers?.Where(n => n.Correct == true).Count();
                    if (count != 1) context.AddFailure("Answers", QuestionErrorMessages.OneAnswerShouldBeCorrect);
                }

                if (e.QuestionType == QuestionType.TrueFalse)
                {
                    if (e.Answers is null || e.Answers.Count != 2)
                    {
                        context.AddFailure("Answers", QuestionErrorMessages.WrongNumberOfCorrectAnswers);
                    }
                }

                if (e.QuestionType == QuestionType.ShortAnswer)
                {
                    if (e.Answers is null || e.Answers.Count == 0)
                        context.AddFailure("Answers", QuestionErrorMessages.WrongNumberOfAnswers);
                }
            });
        }
    }
}
